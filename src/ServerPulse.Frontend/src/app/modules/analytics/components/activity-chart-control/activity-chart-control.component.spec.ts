import { ChangeDetectorRef, NO_ERRORS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { ChartComponent } from 'ng-apexcharts';
import { BehaviorSubject } from 'rxjs';
import { ActivityChartControlComponent } from './activity-chart-control.component';

describe('ActivityChartControlComponent', () => {
  let component: ActivityChartControlComponent;
  let fixture: ComponentFixture<ActivityChartControlComponent>;
  let cdr: ChangeDetectorRef;

  let mockData$: BehaviorSubject<any[]>;
  let mockDateFrom$: BehaviorSubject<Date>;
  let mockDateTo$: BehaviorSubject<Date>;

  beforeEach(async () => {
    mockData$ = new BehaviorSubject([{ x: new Date().getTime(), y: 10 }]);
    mockDateFrom$ = new BehaviorSubject(new Date('2023-01-01T00:00:00Z'));
    mockDateTo$ = new BehaviorSubject(new Date('2023-01-01T01:00:00Z'));

    await TestBed.configureTestingModule({
      declarations: [ActivityChartControlComponent, ChartComponent],
      providers: [ChangeDetectorRef],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ActivityChartControlComponent);
    component = fixture.componentInstance;
    cdr = TestBed.inject(ChangeDetectorRef);

    component.uniqueId = 'test-id';
    component.dateFrom$ = mockDateFrom$.asObservable();
    component.dateTo$ = mockDateTo$.asObservable();
    component.data$ = mockData$.asObservable();

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize chart options on ngOnInit', () => {
    component.ngOnInit();
    expect(component.chartOptions).toBeDefined();
    expect(component.chartOptions.series?.length).toBe(1);
    expect(component.chartOptions.chart?.id).toBe('chart-test-id');
  });

  it('should update chart data when data$ changes', fakeAsync(() => {
    component.ngAfterViewInit();

    const newData = [{ x: new Date().getTime(), y: 20 }];
    mockData$.next(newData);

    tick();
    fixture.detectChanges();

    expect(component.chartOptions.series![0].data).toEqual(newData);
  }));

  it('should update control chart range when date range changes', fakeAsync(() => {
    component.ngAfterViewInit();

    const prevDateFrom = component.chartOptions.xaxis!.min;
    const prevDateTo = component.chartOptions.xaxis!.max;

    tick(300);

    const newDateFrom = new Date('2023-02-01T00:00:00Z');
    const newDateTo = new Date('2023-02-01T01:00:00Z');
    mockDateFrom$.next(newDateFrom);
    mockDateTo$.next(newDateTo);

    expect(component.chartOptions.xaxis!.min).not.toBe(prevDateFrom);
    expect(component.chartOptions.xaxis!.max).not.toBe(prevDateTo);
  }));

  it('should update control chart options when a time range button is clicked', fakeAsync(() => {
    component.updateControlOptions('3m');
    tick();
    fixture.detectChanges();

    const expectedMin = component.currentTime.getTime() - 90 * 24 * 60 * 60 * 1000;
    const expectedMax = component.currentTime.getTime();

    expect(component.chartOptions.xaxis!.min).toBe(expectedMin);
    expect(component.chartOptions.xaxis!.max).toBe(expectedMax);
    expect(component.activeOptionButton).toBe('3m');
  }));

  it('should emit onControlSelect when a data point is selected', () => {
    spyOn(component.onControlSelect, 'emit');

    const mockEvent = new Event('click');
    const chartContext = {};
    const opts = { dataPointIndex: 0 };

    component.chartOptions.chart?.events?.dataPointSelection?.(mockEvent, chartContext, opts);

    expect(component.onControlSelect.emit).toHaveBeenCalledWith(opts);
  });

  it('should clean up on destroy', () => {
    spyOn(component['destroy$'], 'next');
    spyOn(component['destroy$'], 'complete');

    component.ngOnDestroy();

    expect(component['destroy$'].next).toHaveBeenCalled();
    expect(component['destroy$'].complete).toHaveBeenCalled();
  });
});