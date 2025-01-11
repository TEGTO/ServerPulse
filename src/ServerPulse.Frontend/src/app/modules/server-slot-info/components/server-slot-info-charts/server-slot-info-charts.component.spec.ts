/* eslint-disable @typescript-eslint/no-explicit-any */
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { MemoizedSelector, Store } from '@ngrx/store';
import { of } from 'rxjs';
import { addNewLoadEvent, getDailyLoadAmountStatistics, selectLoadAmountStatistics, selectSecondaryLoadAmountStatistics, selectSelectedDate, setLoadStatisticsInterval, setSecondaryLoadStatisticsInterval } from '../..';
import { getDefaultLoadEvent, LoadEvent } from '../../../analyzer';
import { ServerSlotInfoChartsComponent } from './server-slot-info-charts.component';

describe('ServerSlotInfoChartsComponent', () => {
  let component: ServerSlotInfoChartsComponent;
  let fixture: ComponentFixture<ServerSlotInfoChartsComponent>;
  let mockStore: jasmine.SpyObj<Store>;

  const mockStatistics = [
    { dateFrom: new Date(), amountOfEvents: 10 },
    { dateFrom: new Date(new Date().getTime() - 24 * 60 * 60 * 1000), amountOfEvents: 20 },
  ];

  beforeEach(() => {
    mockStore = jasmine.createSpyObj<Store>('Store', ['dispatch', 'select']);

    mockStore.select.and.callFake((selector: MemoizedSelector<object, any>) => {
      if (selector === selectLoadAmountStatistics || selector == selectSecondaryLoadAmountStatistics) {
        return of(mockStatistics);
      }

      if (selector === selectSelectedDate) {
        return of(new Date());
      }

      return of(null);
    });

    TestBed.configureTestingModule({
      declarations: [ServerSlotInfoChartsComponent],
      providers: [{ provide: Store, useValue: mockStore }],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoChartsComponent);
    component = fixture.componentInstance;

    component.slotKey = 'test-slot-key';
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should dispatch getDailyLoadAmountStatistics and set intervals on ngAfterViewInit', () => {
    spyOn(component as any, 'setUpdateTimeIntervals');
    spyOn(component as any, 'fetchControlStatistics');
    spyOn(component as any, 'handleSelectedDateUpdates');
    spyOn(component as any, 'handleNewEvent');

    component.ngAfterViewInit();

    expect(mockStore.dispatch).toHaveBeenCalledWith(getDailyLoadAmountStatistics({ key: 'test-slot-key' }));
    expect(mockStore.dispatch).toHaveBeenCalledWith(setLoadStatisticsInterval({ interval: component.controlIntervalTime }));
    expect(mockStore.dispatch).toHaveBeenCalledWith(setSecondaryLoadStatisticsInterval({ interval: component.secondaryIntervalTime }));
    expect((component as any).setUpdateTimeIntervals).toHaveBeenCalled();
    expect((component as any).fetchControlStatistics).toHaveBeenCalled();
    expect((component as any).handleSelectedDateUpdates).toHaveBeenCalled();
    expect((component as any).handleNewEvent).toHaveBeenCalled();
  });

  it('should fetch control statistics and update controlChartData$', fakeAsync(() => {
    component['fetchControlStatistics']();

    tick();

    const result = component['controlChartDataSubject$'].value.filter(x => x[1] > 0);

    expect(result.length).toEqual(2);
    expect(result[0][1]).toEqual(20);
    expect(result[1][1]).toEqual(10);
  }));

  it('should handle selected date updates and dispatch correct actions', fakeAsync(() => {
    component['handleSelectedDateUpdates']();

    tick();

    const result = component['secondaryChartDataSubject$'].value.filter(x => x[1] > 0);

    expect(result.length).toEqual(1);
    expect(result[0][1]).toEqual(10);
  }));

  it('should handle new events and dispatch addNewLoadEvent', () => {
    const mockEvent: LoadEvent = { ...getDefaultLoadEvent(), id: 'event-id', timestampUTC: new Date() };

    mockStore.select.and.returnValue(of(mockEvent));
    (component as any).handleNewEvent();

    expect(mockStore.dispatch).toHaveBeenCalledWith(addNewLoadEvent({ event: mockEvent }));
  });

  it('should correctly format control chart dates', () => {
    const mockTimestamp = new Date(2024, 0, 1).getTime();
    const formattedDate = component.controlFormatter(mockTimestamp);

    expect(formattedDate).toContain('01');
    expect(formattedDate).toContain('2024');
  });

  it('should correctly format secondary chart times', () => {
    const mockTimestamp = new Date(2024, 0, 1, 14).getTime();
    const formattedTime = component.secondaryFormatter(mockTimestamp);

    expect(formattedTime).toBe('14:00 - 15:00');
  });

  it('should generate time series correctly', () => {
    const fromDate = new Date(2024, 0, 1);
    const toDate = new Date(2024, 0, 3);
    const intervalTime = component.controlIntervalTime;
    const statistics = [
      { dateFrom: fromDate, amountOfEvents: 10 },
      { dateFrom: new Date(2024, 0, 2), amountOfEvents: 20 },
    ];

    const expectedSeries: [number, number][] = [
      [fromDate.getTime(), 10],
      [new Date(2024, 0, 2).getTime(), 20],
      [toDate.getTime(), 0],
    ];

    const series = (component as any).generateTimeSeries(fromDate, toDate, statistics, intervalTime);
    expect(series).toEqual(expectedSeries);
  });
});
