import { ChangeDetectorRef, NO_ERRORS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, Subject } from 'rxjs';
import { PieChartComponent } from './pie-chart.component';

describe('PieChartComponent', () => {
  let component: PieChartComponent;
  let fixture: ComponentFixture<PieChartComponent>;
  let mockChangeDetectorRef: ChangeDetectorRef;

  beforeEach(async () => {
    mockChangeDetectorRef = jasmine.createSpyObj('ChangeDetectorRef', ['detectChanges']);

    await TestBed.configureTestingModule({
      declarations: [PieChartComponent],
      providers: [
        { provide: ChangeDetectorRef, useValue: mockChangeDetectorRef }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PieChartComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize chart options on init', () => {
    component.ngOnInit();

    expect(component.chartOptions).toBeDefined();
    expect(component.chartOptions.series).toEqual([44, 55, 13, 43, 22]);
    expect(component.chartOptions.chart?.type).toBe('pie');
    expect(component.chartOptions.labels).toEqual(['GET', 'POST', 'PUT', 'PATCH', 'DELETE']);
  });

  it('should update chart data when data$ emits', () => {
    const testData = new Map<string, number>([
      ['GET', 60],
      ['POST', 25],
      ['DELETE', 15]
    ]);

    component.data$ = of(testData);
    component.ngOnInit();
    component.ngAfterViewInit();

    component.data$.subscribe(data => {
      component["updateChartData"](data);
      expect(component.chartOptions.series).toEqual([60, 25, 15]);
      expect(component.chartOptions.labels).toEqual(['GET', 'POST', 'DELETE']);
    });
  });

  it('should clean up subscriptions on destroy', () => {
    const nextSpy = spyOn(component['destroy$'], 'next').and.callThrough();
    const completeSpy = spyOn(component['destroy$'], 'complete').and.callThrough();

    component.ngOnDestroy();

    expect(nextSpy).toHaveBeenCalled();
    expect(completeSpy).toHaveBeenCalled();
  });

  it('should unsubscribe from data$ observable on destroy', () => {
    const destroy$ = new Subject<void>();
    const data$ = new Subject<Map<string, number>>();
    component.data$ = data$.asObservable();
    component['destroy$'] = destroy$;

    const updateChartDataSpy = spyOn(component as any, 'updateChartData').and.callThrough();

    component.ngAfterViewInit();

    data$.next(new Map<string, number>([['GET', 100]]));
    expect(updateChartDataSpy).toHaveBeenCalledWith(new Map<string, number>([['GET', 100]]));

    component.ngOnDestroy();

    data$.next(new Map<string, number>([['POST', 50]]));
    expect(updateChartDataSpy.calls.count()).toBe(1);
  });
});