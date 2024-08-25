import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ServerStatisticsService } from '../../../index';
import { ServerSlotInfoChartsComponent } from './server-slot-info-charts.component';

describe('ServerSlotInfoChartsComponent', () => {
  let component: ServerSlotInfoChartsComponent;
  let fixture: ComponentFixture<ServerSlotInfoChartsComponent>;
  let mockStatisticsService: jasmine.SpyObj<ServerStatisticsService>;

  beforeEach(async () => {
    const statisticsServiceSpy = jasmine.createSpyObj('ServerStatisticsService', [
      'getWholeLoadAmountStatisticsInDays',
      'getLastServerLoadStatistics',
      'getLoadAmountStatisticsInRange',
      'setCurrentLoadStatisticsDate',
      'getCurrentLoadStatisticsDate'
    ]);

    statisticsServiceSpy.getWholeLoadAmountStatisticsInDays.and.returnValue(of([]));
    statisticsServiceSpy.getLastServerLoadStatistics.and.returnValue(of(null));
    statisticsServiceSpy.getLoadAmountStatisticsInRange.and.returnValue(of([]));
    statisticsServiceSpy.getCurrentLoadStatisticsDate.and.returnValue(of(new Date()));

    await TestBed.configureTestingModule({
      declarations: [ServerSlotInfoChartsComponent],
      providers: [
        { provide: ServerStatisticsService, useValue: statisticsServiceSpy }
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoChartsComponent);
    component = fixture.componentInstance;
    mockStatisticsService = TestBed.inject(ServerStatisticsService) as jasmine.SpyObj<ServerStatisticsService>;

    component.slotKey = 'test-slot-key';

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should fetch control statistics on initialization', () => {
    const mockStatistics = [
      { date: new Date('2023-01-01T00:00:00Z'), amountOfEvents: 10, collectedDateUTC: new Date(), isInitial: false },
      { date: new Date('2023-01-02T00:00:00Z'), amountOfEvents: 20, collectedDateUTC: new Date(), isInitial: false }
    ];

    mockStatisticsService.getWholeLoadAmountStatisticsInDays.and.returnValue(of(mockStatistics));
    component.ngAfterViewInit();

    expect(mockStatisticsService.getWholeLoadAmountStatisticsInDays).toHaveBeenCalledWith('test-slot-key');
    expect(component['controlChartDataSubject$'].value.length).toBeGreaterThan(0);
  });

  it('should handle empty control statistics data', () => {
    mockStatisticsService.getWholeLoadAmountStatisticsInDays.and.returnValue(of([]));
    component.ngAfterViewInit();

    expect(component['controlChartDataSubject$'].value.length).toBe(365);
  });

  it('should handle last server load statistics correctly', () => {
    const mockLastServerLoadStatistics = {
      key: 'test-slot-key',
      statistics: {
        lastEvent: null,
        isInitial: false,
        collectedDateUTC: new Date(),
        amountOfEvents: 10,
        loadMethodStatistics: null
      }
    };

    mockStatisticsService.getLastServerLoadStatistics.and.returnValue(of(mockLastServerLoadStatistics));
    component.ngAfterViewInit();

    expect(mockStatisticsService.getLastServerLoadStatistics).toHaveBeenCalledWith('test-slot-key');
    expect(component['controlChartDataSubject$'].value.length).toBeGreaterThan(0);
    expect(component['secondaryChartDataSubject$'].value.length).toBeGreaterThan(0);
  });

  it('should update selected date statistics', () => {
    const mockStatistics = [
      { date: new Date('2023-01-01T00:00:00Z'), amountOfEvents: 10, collectedDateUTC: new Date(), isInitial: false },
      { date: new Date('2023-01-02T00:00:00Z'), amountOfEvents: 20, collectedDateUTC: new Date(), isInitial: false }
    ];

    mockStatisticsService.getLoadAmountStatisticsInRange.and.returnValue(of(mockStatistics));
    component.ngAfterViewInit();
    component['currentSelectedDateSubject$'].next(new Date('2023-01-02T00:00:00Z'));

    expect(component['secondaryChartDataSubject$'].value.length).toBeGreaterThan(0);
  });

  it('should update chart data when controlOnSelect is triggered', () => {
    const mockChartData: [number, number][] = [
      [new Date('2023-01-01T00:00:00Z').getTime(), 10],
      [new Date('2023-01-02T00:00:00Z').getTime(), 20]
    ];

    component['controlChartDataSubject$'].next(mockChartData);

    const mockEvent = { dataPointIndex: 1 };
    component.controlOnSelect(mockEvent);

    expect(mockStatisticsService.setCurrentLoadStatisticsDate).toHaveBeenCalledWith(new Date(mockChartData[1][0]));
  });

  it('should handle empty secondary chart data', () => {
    component['secondaryChartDataSubject$'].next([]);
    fixture.detectChanges();

    expect(component['secondaryChartDataSubject$'].value.length).toBe(0);
  });

  it('should format control dates correctly', () => {
    const timestamp = new Date('2023-01-01T00:00:00Z').getTime();
    const formattedDate = component.controlFormatter(timestamp);
    expect(formattedDate).toBe('01.01.2023');
  });

  it('should format secondary chart dates correctly', () => {
    const timestamp = new Date(0).getTime();
    const formattedDate = component.secondaryFormatter(timestamp);
    expect(formattedDate).toContain(':00 -');
  });

  it('should clean up on destroy', () => {
    spyOn(component['destroy$'], 'next');
    spyOn(component['destroy$'], 'complete');

    component.ngOnDestroy();

    expect(component['destroy$'].next).toHaveBeenCalled();
    expect(component['destroy$'].complete).toHaveBeenCalled();
  });
});