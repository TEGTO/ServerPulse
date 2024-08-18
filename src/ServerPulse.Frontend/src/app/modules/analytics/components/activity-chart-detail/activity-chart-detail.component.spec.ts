import { ChangeDetectorRef, NO_ERRORS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { ChartComponent } from 'ng-apexcharts';
import { BehaviorSubject } from 'rxjs';
import { ActivityChartDetailComponent } from './activity-chart-detail.component';

describe('ActivityChartDetailComponent', () => {
  let component: ActivityChartDetailComponent;
  let fixture: ComponentFixture<ActivityChartDetailComponent>;
  let cdr: ChangeDetectorRef;

  let mockControlData$: BehaviorSubject<any[]>;
  let mockSecondaryData$: BehaviorSubject<any[]>;
  let mockControlDateFrom$: BehaviorSubject<Date>;
  let mockControlDateTo$: BehaviorSubject<Date>;
  let mockSecondaryDateFrom$: BehaviorSubject<Date>;
  let mockSecondaryDateTo$: BehaviorSubject<Date>;

  beforeEach(async () => {
    mockControlData$ = new BehaviorSubject([{ x: new Date().getTime(), y: 10 }]);
    mockSecondaryData$ = new BehaviorSubject([{ x: new Date().getTime(), y: 20 }]);
    mockControlDateFrom$ = new BehaviorSubject(new Date('2023-01-01T00:00:00Z'));
    mockControlDateTo$ = new BehaviorSubject(new Date('2023-01-01T01:00:00Z'));
    mockSecondaryDateFrom$ = new BehaviorSubject(new Date('2023-01-01T00:00:00Z'));
    mockSecondaryDateTo$ = new BehaviorSubject(new Date('2023-01-01T01:00:00Z'));

    await TestBed.configureTestingModule({
      declarations: [ActivityChartDetailComponent, ChartComponent],
      providers: [ChangeDetectorRef],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ActivityChartDetailComponent);
    component = fixture.componentInstance;
    cdr = TestBed.inject(ChangeDetectorRef);

    component.chartUniqueId = 'test-id';
    component.controlDateFrom$ = mockControlDateFrom$.asObservable();
    component.controlDateTo$ = mockControlDateTo$.asObservable();
    component.controlData$ = mockControlData$.asObservable();
    component.secondaryDateFrom$ = mockSecondaryDateFrom$.asObservable();
    component.secondaryDateTo$ = mockSecondaryDateTo$.asObservable();
    component.secondaryData$ = mockSecondaryData$.asObservable();

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize chart options on ngOnInit', () => {
    component.ngOnInit();
    expect(component.controlChartOptions).toBeDefined();
    expect(component.secondaryChartOptions).toBeDefined();
  });

  it('should set the correct chart ids based on chartUniqueId input', () => {
    component.ngOnInit();
    expect(component.controlChartOptions.chart!.id).toBe('chart1-test-id');
    expect(component.secondaryChartOptions.chart!.id).toBe('chart2-test-id');
  });

  it('should update control chart range when date range changes', fakeAsync(() => {
    component.ngAfterViewInit();

    const prevDateFrom = component.controlChartOptions.xaxis!.min;
    const prevDateTo = component.controlChartOptions.xaxis!.max;

    tick(300);

    const newDateFrom = new Date('2023-02-01T00:00:00Z');
    const newDateTo = new Date('2023-02-01T01:00:00Z');
    mockControlDateFrom$.next(newDateFrom);
    mockControlDateTo$.next(newDateTo);

    expect(component.controlChartOptions.xaxis!.min).not.toBe(prevDateFrom);
    expect(component.controlChartOptions.xaxis!.max).not.toBe(prevDateTo);
  }));

  it('should update secondary chart range when date range changes', fakeAsync(() => {
    component.ngAfterViewInit();

    const newDateFrom = new Date('2023-02-01T00:00:00Z');
    const newDateTo = new Date('2023-02-01T01:00:00Z');
    mockSecondaryDateFrom$.next(newDateFrom);
    mockSecondaryDateTo$.next(newDateTo);

    tick();
    fixture.detectChanges();

    expect(component.secondaryChartOptions.xaxis!.min).toBe(newDateFrom.getTime());
    expect(component.secondaryChartOptions.xaxis!.max).toBe(newDateTo.getTime());
  }));

  it('should update chart data when control data changes', fakeAsync(() => {
    component.ngAfterViewInit();

    const newData = [{ x: new Date().getTime(), y: 30 }];
    mockControlData$.next(newData);

    tick();
    fixture.detectChanges();

    expect(component.controlChartOptions.series![0].data).toEqual(newData);
  }));

  it('should update chart data when secondary data changes', fakeAsync(() => {
    component.ngAfterViewInit();

    const newData = [{ x: new Date().getTime(), y: 40 }];
    mockSecondaryData$.next(newData);

    tick();
    fixture.detectChanges();

    expect(component.secondaryChartOptions.series![0].data).toEqual(newData);
  }));

  it('should update control chart options when a time range button is clicked', fakeAsync(() => {
    component.updateControlOptions('3m');
    tick();
    fixture.detectChanges();

    const expectedMin = component.currentTime.getTime() - 90 * 24 * 60 * 60 * 1000;
    const expectedMax = component.currentTime.getTime();

    expect(component.controlChartOptions.xaxis!.min).toBe(expectedMin);
    expect(component.controlChartOptions.xaxis!.max).toBe(expectedMax);
    expect(component.activeOptionButton).toBe('3m');
  }));
});