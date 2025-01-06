import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { MemoizedSelector, Store } from '@ngrx/store';
import { of } from 'rxjs';
import { getDailyLoadAmountStatistics, selectLoadAmountStatistics, selectSecondaryLoadAmountStatistics, selectSelectedDate, setSelectedDate, SlotInfoState } from '../..';
import { LoadAmountStatistics } from '../../../analyzer';
import { ServerSlotInfoChartsComponent } from './server-slot-info-charts.component';

describe('ServerSlotInfoChartsComponent', () => {
  let component: ServerSlotInfoChartsComponent;
  let fixture: ComponentFixture<ServerSlotInfoChartsComponent>;
  let mockStore: jasmine.SpyObj<Store>;

  beforeEach(() => {
    mockStore = jasmine.createSpyObj<Store>('Store', ['dispatch', 'select']);

    mockStore.select.and.callFake((selector: MemoizedSelector<object, LoadAmountStatistics[], (s1: SlotInfoState) => LoadAmountStatistics[]> | MemoizedSelector<object, Date, (s1: SlotInfoState) => Date>) => {
      if (selector === selectLoadAmountStatistics || selector === selectSecondaryLoadAmountStatistics) {
        return of([]);
      }
      if (selector === selectSelectedDate) {
        return of(new Date());
      }
      return of(null);
    });

    TestBed.configureTestingModule({
      declarations: [ServerSlotInfoChartsComponent],
      providers: [{ provide: Store, useValue: mockStore }],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(ServerSlotInfoChartsComponent);
    component = fixture.componentInstance;

    component.slotKey = 'test-slot-key';
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should dispatch getDailyLoadAmountStatistics on ngAfterViewInit', () => {
    component.ngAfterViewInit();
    expect(mockStore.dispatch).toHaveBeenCalledWith(
      getDailyLoadAmountStatistics({ key: 'test-slot-key' })
    );
  });

  it('should fetch control statistics and update controlChartData$', () => {
    const oneDay = 1000 * 60 * 60 * 24;
    const mockStatistics = [
      { dateFrom: new Date(new Date().getTime() - oneDay), amountOfEvents: 20 },
      { dateFrom: new Date(new Date().getTime() - 2 * oneDay), amountOfEvents: 10 },
    ];

    mockStore.select.and.returnValue(of(mockStatistics));
    component["fetchControlStatistics"]();

    const result = component["controlChartDataSubject$"].value.filter(x => x[1] > 0);

    expect(result.length).toEqual(2);
    expect(Math.abs(result[1][0] - result[0][0])).toEqual(oneDay);
    expect(result[0][1]).toEqual(20);
    expect(result[1][1]).toEqual(10);
  });

  it('should update secondary chart data when selected date changes', fakeAsync(() => {
    const mockDate = new Date();
    const oneHour = 1000 * 60 * 60;
    const mockStatistics = [
      { dateFrom: new Date(new Date()), amountOfEvents: 15 },
      { dateFrom: new Date(new Date().getTime() + oneHour), amountOfEvents: 25 },
    ];

    mockStore.select.and.returnValues(of(mockDate), of(mockStatistics));

    component["handleSelectedDateUpdates"]();

    tick();

    const result = component["secondaryChartDataSubject$"].value.filter(x => x[1] > 0);

    expect(result.length).toEqual(2);
    expect(result[1][0] - result[0][0]).toEqual(oneHour);
    expect(result[0][1]).toEqual(15);
    expect(result[1][1]).toEqual(25);
  }));

  it('should dispatch setSelectedDate on controlOnSelect', () => {
    const mockChartData: [number, number][] = [[1672531200000, 5]];
    component["controlChartDataSubject$"].next(mockChartData);

    const mockEvent = { dataPointIndex: 0 };
    component.controlOnSelect(mockEvent);

    const expectedDate = new Date(mockChartData[0][0]);
    const expectedReadFromDate = new Date(expectedDate);
    expectedReadFromDate.setHours(23, 59, 59, 999);

    expect(mockStore.dispatch).toHaveBeenCalledWith(
      setSelectedDate({ date: expectedDate, readFromDate: expectedReadFromDate })
    );
  });

  it('should add event to chart data in addEventToChartData', () => {
    const mockChartData: [number, number][] = [[1672531200000, 5]];
    const loadTime = 1672534800000; // Within the interval

    const updatedChartData = component["addEventToChartData"](
      loadTime,
      mockChartData,
      component.controlIntervalTime
    );

    expect(updatedChartData).toEqual([[1672531200000, 6]]);
  });

  it('should generate correct time series', () => {
    const fromDate = new Date(2024, 0, 1);
    const toDate = new Date(2024, 0, 3);
    const intervalTime = 24 * 60 * 60 * 1000; // 1 day
    const statistics = new Map<number, number>([
      [fromDate.getTime(), 10],
      [new Date(2024, 0, 2).getTime(), 20],
    ]);

    const expectedSeries: [number, number][] = [
      [fromDate.getTime(), 10],
      [new Date(2024, 0, 2).getTime(), 20],
      [toDate.getTime(), 0],
    ];

    const series = component["generateTimeSeries"](fromDate, toDate, intervalTime, statistics);
    expect(series).toEqual(expectedSeries);
  });

  it('should format control chart dates', () => {
    const mockTimestamp = new Date(2024, 0, 1).getTime();
    const formattedDate = component.controlFormatter(mockTimestamp);

    expect(formattedDate).toContain('01');
    expect(formattedDate).toContain('2024');
  });

  it('should format secondary chart times', () => {
    const mockTimestamp = new Date(2024, 0, 1, 14).getTime();
    const formattedTime = component.secondaryFormatter(mockTimestamp);

    expect(formattedTime).toBe('14:00 - 15:00');
  });
});
