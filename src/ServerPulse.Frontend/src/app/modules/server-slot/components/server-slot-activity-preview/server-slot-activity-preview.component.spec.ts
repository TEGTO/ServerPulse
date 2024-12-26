/* eslint-disable @typescript-eslint/no-explicit-any */
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { LoadAmountStatistics, startLoadStatisticsReceiving, stopLoadKeyListening } from '../../../analyzer';
import { ServerSlot } from '../../../server-slot-shared';
import { ServerSlotActivityPreviewComponent } from './server-slot-activity-preview.component';

describe('ServerSlotActivityPreviewComponent', () => {
  let component: ServerSlotActivityPreviewComponent;
  let fixture: ComponentFixture<ServerSlotActivityPreviewComponent>;

  let storeSpy: jasmine.SpyObj<Store>;

  const mockServerSlot: ServerSlot = {
    id: 'slot1',
    userEmail: 'user@example.com',
    name: 'Test Slot',
    slotKey: 'slot-key-1',
  };

  const mockLoadStatistics: LoadAmountStatistics[] = [
    { id: 'stat1', collectedDateUTC: new Date(), amountOfEvents: 5, dateFrom: new Date(), dateTo: new Date() },
    { id: 'stat2', collectedDateUTC: new Date(), amountOfEvents: 3, dateFrom: new Date(), dateTo: new Date() },
  ];

  beforeEach(async () => {
    storeSpy = jasmine.createSpyObj<Store>(['dispatch', 'select']);
    storeSpy.select.and.returnValue(of(mockLoadStatistics));

    await TestBed.configureTestingModule({
      declarations: [ServerSlotActivityPreviewComponent],
      providers: [
        { provide: Store, useValue: storeSpy },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotActivityPreviewComponent);
    component = fixture.componentInstance;

    component.serverSlot = mockServerSlot;
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should dispatch getLoadAmountStatisticsInRange action', () => {
      expect(storeSpy.dispatch).toHaveBeenCalled();
    });

    it('should dispatch startLoadStatisticsReceiving action', () => {
      expect(storeSpy.dispatch).toHaveBeenCalledWith(startLoadStatisticsReceiving({ key: 'slot-key-1', getInitial: false }));
    });
  });

  describe('ngOnDestroy', () => {
    it('should dispatch stopLoadKeyListening action', () => {
      component.serverSlot = mockServerSlot;
      component.ngOnDestroy();
      expect(storeSpy.dispatch).toHaveBeenCalledWith(stopLoadKeyListening({ key: 'slot-key-1' }));
    });

    it('should complete destroy$ subject', () => {
      const completeSpy = spyOn(component['destroy$'], 'complete');
      component.ngOnDestroy();
      expect(completeSpy).toHaveBeenCalled();
    });
  });

  describe('setChartDataObservable', () => {
    it('should update chart data observable with time series', () => {
      const mockStatisticsSet = new Map<number, number>([
        [new Date().getTime(), 5],
        [new Date().getTime() + component.fiveMinutes, 3],
      ]);
      spyOn<any>(component, 'getStatisticsSet').and.returnValue(mockStatisticsSet);
      spyOn<any>(component, 'generate5MinutesTimeSeries').and.returnValue([[0, 5], [1, 3]]);

      component.serverSlot = mockServerSlot;
      component["setChartDataObservable"]();

      component.chartData$.subscribe((data) => {
        expect(data).toEqual([[0, 5], [1, 3]]);
      });
    });

    it('should dispatch addLoadEventToLoadAmountStatistics when last load event is available', () => {
      const mockEvent = { id: 'event1', creationDateUTC: new Date() };
      storeSpy.select.and.returnValues(of(mockLoadStatistics), of(mockEvent));

      component.serverSlot = mockServerSlot;
      component["setChartDataObservable"]();

      expect(storeSpy.dispatch).toHaveBeenCalled();
    });
  });

  describe('updateTime', () => {
    it('should update dateFromSubject$ and dateToSubject$ values', () => {
      const now = new Date();
      spyOn<any>(component, 'getDateFrom').and.returnValue(now);

      component["updateTime"]();

      component.dateFrom$.subscribe((date) => {
        expect(date).toEqual(now);
      });
      component.dateTo$.subscribe((date) => {
        expect(date).toBeInstanceOf(Date);
      });
    });
  });

  describe('generate5MinutesTimeSeries', () => {
    it('should generate a time series based on statistics', () => {
      const startDate = new Date().getTime();
      const mockStatisticsSet = new Map<number, number>([
        [startDate, 5],
        [startDate + component.fiveMinutes, 3],
      ]);

      const result = component["generate5MinutesTimeSeries"](
        new Date(),
        new Date(Date.now() + component.fiveMinutes * 2),
        mockStatisticsSet
      );

      expect(result).toEqual([
        [startDate, 5],
        [startDate + component.fiveMinutes, 3],
        [startDate + 2 * component.fiveMinutes, 0],
      ]);
    });
  });
});
