/* eslint-disable @typescript-eslint/no-explicit-any */
import { ChangeDetectorRef, NO_ERRORS_SCHEMA } from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { of } from 'rxjs';
import { ActivityChartComponent } from './activity-chart.component';

describe('ActivityChartComponent', () => {
  let component: ActivityChartComponent;
  let fixture: ComponentFixture<ActivityChartComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ActivityChartComponent],
      providers: [ChangeDetectorRef],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ActivityChartComponent);
    component = fixture.componentInstance;

    component.uniqueId = 'test-id';
    component.dateFrom$ = of(new Date('2023-01-01T00:00:00Z'));
    component.dateTo$ = of(new Date('2023-01-01T01:00:00Z'));
    component.data$ = of([{ x: new Date().getTime(), y: 10 }]);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize chart options on ngOnInit', () => {
    component.ngOnInit();
    expect(component.chartOptions).toBeDefined();
    expect(component.chartOptions.series).toBeDefined();
    expect(component.chartOptions.chart).toBeDefined();
    expect(component.chartOptions.xaxis).toBeDefined();
    expect(component.chartOptions.yaxis).toBeDefined();
  });

  it('should set the chart id based on chartUniqueId input', () => {
    component.uniqueId = 'test-id';
    component.ngOnInit();
    expect(component.chartOptions.chart!.id).toBe('chart-test-id');
  });

  it('should update chart range when date range changes', fakeAsync(() => {
    const dateFrom = new Date('2023-01-01T00:00:00Z');
    const dateTo = new Date('2023-01-01T01:00:00Z');
    component.ngOnInit();

    component['updateChartRange'](dateFrom, dateTo);
    tick();

    expect(component.chartOptions.xaxis!.min).toBe(dateFrom.getTime());
    expect(component.chartOptions.xaxis!.max).toBe(dateTo.getTime());
  }));

  it('should update chart data when data changes', fakeAsync(() => {
    const mockData = [{ x: new Date(1).getTime(), y: 10 }];

    component.ngOnInit();

    component['updateChartData'](mockData);

    tick();

    expect(component.chartOptions.series![0].data).toEqual(mockData);
  }));

  it('should subscribe to dateFrom$, dateTo$, and data$ observables', fakeAsync(() => {
    const updateChartRange = spyOn<any>(component, 'updateChartRange');
    const updateChartData = spyOn<any>(component, 'updateChartData');

    const mockDateFrom$ = of(new Date('2023-01-01T00:00:00Z'));
    const mockDateTo$ = of(new Date('2023-01-01T01:00:00Z'));
    const mockData$ = of([{ x: new Date().getTime(), y: 10 }]);

    component.dateFrom$ = mockDateFrom$;
    component.dateTo$ = mockDateTo$;
    component.data$ = mockData$;

    component.ngOnInit();
    tick();

    expect(updateChartRange).toHaveBeenCalled();
    expect(updateChartData).toHaveBeenCalled();
  }));
});