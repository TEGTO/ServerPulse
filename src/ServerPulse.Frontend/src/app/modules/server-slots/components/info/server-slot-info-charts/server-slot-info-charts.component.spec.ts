import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ServerStatisticsService } from '../..//index';
import { ServerSlotInfoChartsComponent } from './server-slot-info-charts.component';

describe('ServerSlotInfoChartsComponent', () => {
  let component: ServerSlotInfoChartsComponent;
  let fixture: ComponentFixture<ServerSlotInfoChartsComponent>;
  let mockStatisticsService: jasmine.SpyObj<ServerStatisticsService>;

  beforeEach(async () => {
    const statisticsServiceSpy = jasmine.createSpyObj('ServerStatisticsService', [
      'getWholeAmountStatisticsInDays',
      'getLastServerLoadStatistics',
      'getAmountStatisticsInRange',
      'setCurrentLoadStatisticsDate'
    ]);

    const mockStatistics = [
      { date: new Date('2023-01-01T00:00:00Z'), amountOfEvents: 10 },
      { date: new Date('2023-01-02T00:00:00Z'), amountOfEvents: 20 },
    ];

    statisticsServiceSpy.getWholeAmountStatisticsInDays.and.returnValue(of(mockStatistics));
    statisticsServiceSpy.getLastServerLoadStatistics.and.returnValue(of(mockStatistics[0]));

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
      { date: new Date('2023-01-01T00:00:00Z'), amountOfEvents: 10 },
      { date: new Date('2023-01-02T00:00:00Z'), amountOfEvents: 20 },
    ];
    const mockServerStatistics =
    {
      key: 'test-slot-key',
      statistics: {
        lastEvent: null,
        isInitial: false,
        amountOfEvents: 10,
        collectedDateUTC: new Date()
      }
    }

    mockStatisticsService.getWholeLoadAmountStatisticsInDays.and.returnValue(of(mockStatistics));
    mockStatisticsService.getLoadAmountStatisticsInRange.and.returnValue(of(mockStatistics));
    mockStatisticsService.getLastServerLoadStatistics.and.returnValue(of(mockServerStatistics));

    component.ngAfterViewInit();

    expect(mockStatisticsService.getWholeLoadAmountStatisticsInDays).toHaveBeenCalledWith('test-slot-key');
    expect(mockStatisticsService.getLoadAmountStatisticsInRange).toHaveBeenCalled();
    expect(mockStatisticsService.getLastServerLoadStatistics).toHaveBeenCalled();
    expect(component["controlStatisticsSetSubject$"].getValue().size).toBe(2);
  });

  it('should update control chart data when statistics are fetched', () => {
    const mockStatistics = [
      { date: new Date('2023-01-01T00:00:00Z'), amountOfEvents: 10 },
      { date: new Date('2023-01-02T00:00:00Z'), amountOfEvents: 20 },
    ];
    const mockServerStatistics =
    {
      key: 'test-slot-key',
      statistics: {
        lastEvent: null,
        isInitial: false,
        amountOfEvents: 10,
        collectedDateUTC: new Date()
      }
    }

    mockStatisticsService.getWholeLoadAmountStatisticsInDays.and.returnValue(of(mockStatistics));
    mockStatisticsService.getLoadAmountStatisticsInRange.and.returnValue(of(mockStatistics));
    mockStatisticsService.getLastServerLoadStatistics.and.returnValue(of(mockServerStatistics));

    component.ngAfterViewInit();

    const chartData = component["controlChartDataSubject$"].getValue();
    expect(chartData.length).toBeGreaterThan(0);
  });

  it('should handle last server load statistics correctly', () => {
    const mockStatistics = [
      { date: new Date('2023-01-01T00:00:00Z'), amountOfEvents: 10 },
      { date: new Date('2023-01-02T00:00:00Z'), amountOfEvents: 20 },
    ];
    const mockLastServerLoadStatistics = {
      key: 'test-slot-key',
      statistics: {
        lastEvent: null,
        isInitial: false,
        amountOfEvents: 10,
        collectedDateUTC: new Date()
      }
    };

    mockStatisticsService.getLastServerLoadStatistics.and.returnValue(of(mockLastServerLoadStatistics));
    mockStatisticsService.getLoadAmountStatisticsInRange.and.returnValue(of(mockStatistics));

    component.ngAfterViewInit();

    expect(mockStatisticsService.getLastServerLoadStatistics).toHaveBeenCalledWith('test-slot-key');
    expect(component["controlChartDataSubject$"].getValue().length).toBeGreaterThan(0);
    expect(component["secondaryChartDataSubject$"].getValue().length).toBeGreaterThan(0);
  });

  it('should update selected date statistics', () => {
    const mockStatistics = [
      { date: new Date('2023-01-01T00:00:00Z'), amountOfEvents: 10 },
      { date: new Date('2023-01-02T00:00:00Z'), amountOfEvents: 20 },
    ];

    mockStatisticsService.getLoadAmountStatisticsInRange.and.returnValue(of(mockStatistics));

    component.ngAfterViewInit();

    component["currentSelectedDateSubject$"].next(new Date('2023-01-02T00:00:00Z'));

    const secondaryChartData = component["secondaryChartDataSubject$"].getValue();
    expect(secondaryChartData.length).toBeGreaterThan(0);
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

  it('should clean up on destroy', () => {
    spyOn(component["destroy$"], 'next');
    spyOn(component["destroy$"], 'complete');

    component.ngOnDestroy();

    expect(component["destroy$"].next).toHaveBeenCalled();
    expect(component["destroy$"].complete).toHaveBeenCalled();
  });
});