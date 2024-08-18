import { NO_ERRORS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
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
    lastEvent: null
  };

  beforeEach(async () => {
    mockStatisticsService = jasmine.createSpyObj('ServerStatisticsService', [
      'getAmountStatisticsInRange',
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

    mockStatisticsService.getAmountStatisticsInRange.and.returnValue(of([]));
    mockStatisticsService.getLastServerLoadStatistics.and.returnValue(of({ key: "", statistics: mockLoadStatisticsResponse }));

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set dateFrom and dateTo on initialization', () => {
    expect(component['dateFromSubject$'].value).toBeDefined();
    expect(component['dateToSubject$'].value).toBeDefined();
  });

  it('should update statistics set on service data', () => {
    const statistics = [{ date: new Date(), amountOfEvents: 5 }];
    const updateStatisticsSetSpy = spyOn(component as any, 'updateStatisticsSet').and.callThrough();

    mockStatisticsService.getAmountStatisticsInRange.and.returnValue(of(statistics));

    component.ngAfterViewInit();

    expect(updateStatisticsSetSpy).toHaveBeenCalledWith(jasmine.any(Map), statistics);
  });

  it('should update chart data when statistics set changes', () => {
    const generate5MinutesTimeSeriesSpy = spyOn(component as any, 'generate5MinutesTimeSeries').and.callThrough();

    component.ngAfterViewInit();

    component['statisticsSetSubject$'].next(new Map<number, number>());

    expect(generate5MinutesTimeSeriesSpy).toHaveBeenCalled();
  });

  it('should update chart data with the last event time', () => {
    const addEventToChartDataSpy = spyOn(component as any, 'addEventToChartData').and.callThrough();
    addEventToChartDataSpy.call(component, new Date().getTime());

    component.ngAfterViewInit();

    component['chartDataSubject$'].next([]);

    expect(addEventToChartDataSpy).toHaveBeenCalledWith(jasmine.any(Number));
  });

  it('should correctly validate incoming messages', () => {
    const validateMessageSpy = spyOn(component as any, 'validateMessage').and.callThrough();
    validateMessageSpy.call(component, mockLoadStatisticsResponse);

    component.ngAfterViewInit();

    expect(validateMessageSpy).toHaveBeenCalledWith(mockLoadStatisticsResponse);
  });

  it('should clean up subscriptions on destroy', () => {
    const nextSpy = spyOn(component['destroy$'], 'next').and.callThrough();
    const completeSpy = spyOn(component['destroy$'], 'complete').and.callThrough();

    component.ngOnDestroy();

    expect(nextSpy).toHaveBeenCalled();
    expect(completeSpy).toHaveBeenCalled();
  });

  it('should call updateTime on interval', () => {
    const updateTimeSpy = spyOn(component as any, 'updateTime').and.callThrough();

    component.ngAfterViewInit();

    expect(updateTimeSpy).toHaveBeenCalled();
  });
});