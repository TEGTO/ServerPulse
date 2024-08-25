import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { of } from 'rxjs';
import { ServerLoadStatisticsResponse, ServerSlot } from '../../../shared';
import { ServerStatisticsService } from '../../index';
import { ServerSlotPreviewActivityChartComponent } from './server-slot-preview-activity-chart.component';

describe('ServerSlotPreviewActivityChartComponent', () => {
  let component: ServerSlotPreviewActivityChartComponent;
  let fixture: ComponentFixture<ServerSlotPreviewActivityChartComponent>;
  let mockStatisticsService: jasmine.SpyObj<ServerStatisticsService>;

  const mockServerSlot: ServerSlot = {
    id: '1',
    name: 'Test Slot',
    slotKey: 'testKey',
    userEmail: 'test@example.com'
  };

  const mockLoadStatisticsResponse: ServerLoadStatisticsResponse = {
    isInitial: false,
    amountOfEvents: 10,
    collectedDateUTC: new Date(),
    lastEvent: null,
    loadMethodStatistics: null,
  };

  beforeEach(async () => {
    mockStatisticsService = jasmine.createSpyObj('ServerStatisticsService', [
      'getLoadAmountStatisticsInRange',
      'getLastServerLoadStatistics'
    ]);

    await TestBed.configureTestingModule({
      declarations: [ServerSlotPreviewActivityChartComponent],
      providers: [
        { provide: ServerStatisticsService, useValue: mockStatisticsService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ServerSlotPreviewActivityChartComponent);
    component = fixture.componentInstance;
    component.serverSlot = mockServerSlot;

    mockStatisticsService.getLoadAmountStatisticsInRange.and.returnValue(of([]));
    mockStatisticsService.getLastServerLoadStatistics.and.returnValue(of({ key: mockServerSlot.slotKey, statistics: mockLoadStatisticsResponse }));

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set dateFrom and dateTo on initialization', () => {
    expect(component['dateFromSubject$'].value).toBeDefined();
    expect(component['dateToSubject$'].value).toBeDefined();
  });

  it('should correctly initialize observables on init', () => {
    spyOn(component as any, 'setUpdateTimeInterval').and.callThrough();
    component.ngOnInit();
    expect(component['setUpdateTimeInterval']).toHaveBeenCalled();
  });

  it('should update chart data with the last event time', fakeAsync(() => {
    const addEventToChartDataSpy = spyOn(component as any, 'addEventToChartData').and.callThrough();
    mockStatisticsService.getLastServerLoadStatistics.and.returnValue(of({
      key: mockServerSlot.slotKey,
      statistics: mockLoadStatisticsResponse
    }));

    component.ngOnInit();

    tick();

    component.chartData$.subscribe(data => {
      expect(addEventToChartDataSpy).toHaveBeenCalledWith(jasmine.any(Array), jasmine.any(Number));
    });

    component.ngOnDestroy();
  }));

  it('should call updateTime on interval', fakeAsync(() => {
    const updateTimeSpy = spyOn(component as any, 'updateTime').and.callThrough();
    component.ngOnInit();
    tick(component.fiveMinutes);
    expect(updateTimeSpy).toHaveBeenCalled();
    component.ngOnDestroy();
  }));

  it('should format dates correctly', () => {
    const date = new Date(2023, 1, 1, 12, 0, 0); // 12:00 PM
    const formatted = component.formatter(date.getTime());
    expect(formatted).toBe('12:00 - 12:05');
  });

  it('should clean up subscriptions on destroy', () => {
    const nextSpy = spyOn(component['destroy$'], 'next').and.callThrough();
    const completeSpy = spyOn(component['destroy$'], 'complete').and.callThrough();
    component.ngOnDestroy();
    expect(nextSpy).toHaveBeenCalled();
    expect(completeSpy).toHaveBeenCalled();
  });

  it('should update statistics set on service data', fakeAsync(() => {
    const statistics = [{ date: new Date(), amountOfEvents: 5, collectedDateUTC: new Date(), isInitial: false }];
    const getStatisticsSetSpy = spyOn(component as any, 'getStatisticsSet').and.callThrough();

    mockStatisticsService.getLoadAmountStatisticsInRange.and.returnValue(of(statistics));

    component.ngOnInit();

    tick();

    component.chartData$.subscribe(data => {
      expect(getStatisticsSetSpy).toHaveBeenCalledWith(statistics);
    });

    component.ngOnDestroy();
  }));

  it('should generate time series correctly', () => {
    const dateFrom = new Date(Date.now() - component.hour);
    const dateTo = new Date();
    const statisticsSet = new Map<number, number>([[dateFrom.getTime(), 5]]);
    const series = component['generate5MinutesTimeSeries'](dateFrom, dateTo, statisticsSet);

    expect(series.length).toBeGreaterThan(0);
    expect(series[0][1]).toBe(5);
  });

  it('should validate incoming messages correctly', () => {
    const validMessage = { key: mockServerSlot.slotKey, statistics: mockLoadStatisticsResponse };
    const invalidMessage = { key: 'wrongKey', statistics: mockLoadStatisticsResponse };
    const isValid = component['validateMessage'](validMessage);
    const isInvalid = component['validateMessage'](invalidMessage);

    expect(isValid).toBeTrue();
    expect(isInvalid).toBeFalse();
  });
});