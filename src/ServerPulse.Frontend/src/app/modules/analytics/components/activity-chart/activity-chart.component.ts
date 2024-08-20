import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnInit, ViewChild } from '@angular/core';
import { ChartComponent, ChartType } from 'ng-apexcharts';
import { combineLatest, Observable } from 'rxjs';
import { ChartOptions } from '../../index';

export enum ActivityChartType {
  Line,
  Box,
}
@Component({
  selector: 'activity-chart',
  templateUrl: './activity-chart.component.html',
  styleUrls: ['./activity-chart.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivityChartComponent implements OnInit, AfterViewInit {
  @Input({ required: true }) uniqueId!: string;
  @Input({ required: true }) dateFrom$!: Observable<Date>;
  @Input({ required: true }) dateTo$!: Observable<Date>;
  @Input({ required: true }) data$!: Observable<any[]>;
  @Input({ required: true }) chartType?: ActivityChartType;
  @Input({ required: true }) formatter?: (val: number, opts?: any) => string;
  @ViewChild('chart') chart!: ChartComponent;

  chartOptions!: Partial<ChartOptions>;

  get chartId() { return `chart-${this.uniqueId}`; }

  constructor(
    private readonly cdr: ChangeDetectorRef,
  ) { }

  ngOnInit(): void {
    this.initChartOptions();
  }

  ngAfterViewInit(): void {
    combineLatest([this.dateFrom$, this.dateTo$]).subscribe(
      ([dateFrom, dateTo]) => this.updateChartRange(dateFrom, dateTo)
    );
    this.data$.subscribe(
      (data) => this.updateChartData(data)
    );
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
    if (this.chartOptions != null && this.chartOptions.series != null) {
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
    switch (this.chartType) {
      case ActivityChartType.Line:
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
      case ActivityChartType.Box:
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
      default:
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
  }
}