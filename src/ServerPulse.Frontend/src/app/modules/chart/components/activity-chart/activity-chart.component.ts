/* eslint-disable @typescript-eslint/no-explicit-any */
import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ChartComponent, ChartType } from 'ng-apexcharts';
import { Observable, Subject, combineLatest, takeUntil } from 'rxjs';
import { ChartOptions } from '../..';

export enum ActivityChartType {
  Line,
  Box,
}

@Component({
  selector: 'app-activity-chart',
  templateUrl: './activity-chart.component.html',
  styleUrl: './activity-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivityChartComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input({ required: true }) uniqueId!: string;
  @Input({ required: true }) dateFrom$!: Observable<Date>;
  @Input({ required: true }) dateTo$!: Observable<Date>;
  @Input({ required: true }) data$!: Observable<any[]>;
  @Input({ required: true }) chartType?: ActivityChartType;
  @Input({ required: true }) formatter?: (val: number, opts?: any) => string;
  @ViewChild('chart') chart!: ChartComponent;

  chartOptions!: Partial<ChartOptions>;
  private readonly destroy$ = new Subject<void>();

  get chartId() { return `chart-${this.uniqueId}`; }

  constructor(
    private readonly cdr: ChangeDetectorRef,
  ) { }

  ngOnInit(): void {
    this.initChartOptions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngAfterViewInit(): void {
    combineLatest([this.dateFrom$, this.dateTo$])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([dateFrom, dateTo]) => this.updateChartRange(dateFrom, dateTo));

    this.data$
      .pipe(takeUntil(this.destroy$))
      .subscribe((data) => this.updateChartData(data));
  }

  private initChartOptions() {
    this.chartOptions = {
      ...this.getChartTypeOptions(),
      series: [
        {
          name: "Events",
          data: []
        }
      ],
      dataLabels: {
        enabled: false
      },
      markers: {
        size: 0
      },
      xaxis: {
        type: "datetime",
        min: new Date().getTime(),
        max: new Date().getTime(),
        labels: {
          datetimeUTC: false,
          format: 'HH:mm'
        },
        tickAmount: 12,
        tooltip: {
          enabled: false
        }
      },
      yaxis: {
        show: true,
        title: {
          text: "Number of Events"
        }
      },
      tooltip: {
        x: {
          formatter: this.formatter
        },
        marker: {
          show: true,
          fillColors: ['#40abfc']
        }
      },
    };
  }

  private updateChartData(data: any[]) {
    if (this.chartOptions?.series != null) {
      this.chartOptions.series =
        [
          {
            name: "Events",
            data: data
          }
        ];
      this.cdr.detectChanges();
    }
  }

  private updateChartRange(dateFrom: Date, dateTo: Date) {
    if (this.chart && this.chartOptions.xaxis) {
      this.chartOptions.xaxis =
      {
        type: "datetime",
        min: dateFrom.getTime(),
        max: dateTo.getTime(),
        labels: {
          datetimeUTC: false,
          format: 'HH:mm'
        },
        tickAmount: 12,
        tooltip: {
          enabled: false
        }
      };
      this.cdr.detectChanges();
    }
  }

  private getChartTypeOptions(): Partial<ChartOptions> {
    if (this.chartType == ActivityChartType.Line) {
      return {
        chart: {
          id: this.chartId,
          type: 'area' as ChartType,
          height: 220,
          stacked: false,
          toolbar: {
            autoSelected: 'pan',
            show: false
          },
          animations: {
            enabled: true,
          }
        },
        stroke: {
          width: [4],
          curve: ['smooth']
        },
        fill: {
          type: 'gradient',
          gradient: {
            shadeIntensity: 1,
            inverseColors: false,
            opacityFrom: 0.45,
            opacityTo: 0.05,
            stops: [20, 100, 100, 100]
          }
        }
      };
    }
    else {
      return {
        chart: {
          id: this.chartId,
          type: 'bar' as ChartType,
          height: 260,
          stacked: false,
          toolbar: {
            autoSelected: 'pan',
            show: false
          },
          animations: {
            enabled: true,
          }
        },
        fill: {
          type: 'gradient',
          gradient: {
            shade: 'light',
            type: 'horizontal',
            shadeIntensity: 0.25,
            inverseColors: true,
            opacityFrom: 1,
            opacityTo: 1,
            stops: [50, 0, 100, 100]
          }
        }
      };
    }
  }
}