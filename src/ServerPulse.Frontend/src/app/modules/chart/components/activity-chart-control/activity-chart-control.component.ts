/* eslint-disable @typescript-eslint/no-explicit-any */
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { ChartComponent } from 'ng-apexcharts';
import { combineLatest, Observable, Subject, takeUntil } from 'rxjs';
import { ChartOptions } from '../..';

type TimeRange = "1w" | "1m" | "3m" | "6m" | "all";

@Component({
  selector: 'app-activity-chart-control',
  templateUrl: './activity-chart-control.component.html',
  styleUrl: './activity-chart-control.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivityChartControlComponent implements OnInit, OnDestroy {
  @Input({ required: true }) uniqueId!: string;
  @Input({ required: true }) dateFrom$!: Observable<Date>;
  @Input({ required: true }) dateTo$!: Observable<Date>;
  @Input({ required: true }) data$!: Observable<any[]>;
  @Output() controlSelect = new EventEmitter<any>();
  @Input({ required: true }) formatter?: (val: number, opts?: any) => string;
  @ViewChild('chart') chart!: ChartComponent;

  chartOptions!: Partial<ChartOptions>;
  activeOptionButton: TimeRange = "1w";
  private data: any[] = [];
  private readonly dateFrom: Date = new Date(this.currentTime.getTime() - this.week);
  private readonly dateTo: Date = this.currentTime;
  private readonly destroy$ = new Subject<void>();

  get chartId() { return `chart-${this.uniqueId}`; }
  get currentTime() { return new Date(); }
  get week() { return 7 * 24 * 60 * 60 * 1000; }
  get updateOptionsData() {
    return {
      "1w": { xaxis: { min: this.currentTime.getTime() - 7 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
      "1m": { xaxis: { min: this.currentTime.getTime() - 30 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
      "3m": { xaxis: { min: this.currentTime.getTime() - 90 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
      "6m": { xaxis: { min: this.currentTime.getTime() - 180 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
      all: { xaxis: { min: undefined, max: undefined } }
    };
  }

  constructor(
    private readonly cdr: ChangeDetectorRef,
  ) { }

  ngOnInit(): void {
    this.initChartOptions();

    this.data$
      .pipe(takeUntil(this.destroy$))
      .subscribe((data) => {
        this.data = data;
        this.updateChartData(this.chartOptions, data);
      });
    combineLatest([this.dateFrom$, this.dateTo$])
      .pipe(takeUntil(this.destroy$))
      // eslint-disable-next-line @typescript-eslint/no-unused-vars
      .subscribe(([dateFrom, dateTo]) => this.updateChartRange());
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  initChartOptions(): void {
    this.chartOptions = {
      series: [
        {
          name: "Events",
          data: []
        }
      ],
      chart: {
        id: this.chartId,
        type: "bar",
        height: 260,
        stacked: false,
        zoom: {
          type: "x",
          enabled: true,
          autoScaleYaxis: true
        },
        selection: {
          enabled: false
        },
        toolbar: {
          autoSelected: "zoom"
        },
        animations:
        {
          enabled: false
        },
        events: {
          dataPointSelection: (event, chartContext, opts) => {
            this.controlSelect.emit(opts);
          }
        }
      },
      fill: {
        type: "gradient",
        gradient: {
          shade: "light",
          type: "horizontal",
          shadeIntensity: 0.25,
          gradientToColors: undefined,
          inverseColors: true,
          opacityFrom: 1,
          opacityTo: 1,
          stops: [50, 0, 100, 100]
        }
      },
      dataLabels: {
        enabled: false
      },
      xaxis: {
        type: "datetime",
        min: this.dateFrom.getTime(),
        max: this.dateTo.getTime(),
        tooltip: {
          enabled: false,
        }
      },
      tooltip:
      {
        x: {
          formatter: this.formatter
        },
        marker: {
          show: true,
          fillColors: ['#40abfc']
        }
      },
      yaxis: {
        tickAmount: 2,
        title: {
          text: "Event Amount"
        }
      }
    };
  }

  private updateChartData(chartOptions: Partial<ChartOptions>, data: any): void {
    const filteredData = this.filterDataByTimeRange(data, this.activeOptionButton);
    if (chartOptions?.series != null) {
      chartOptions.series =
        [
          {
            name: "Events",
            data: filteredData
          }
        ];
      this.cdr.detectChanges();
    }
  }

  private updateChartRange(): void {
    if (this.chart && this.chartOptions.xaxis) {
      this.updateControlOptions(this.activeOptionButton);
      this.updateChartData(this.chartOptions, this.data);
      this.cdr.detectChanges();
    }
  }

  private filterDataByTimeRange(data: any[], timeRange: TimeRange): any[] {
    const rangeLimits = this.updateOptionsData[timeRange];
    if (!rangeLimits.xaxis.min || !rangeLimits.xaxis.max) {
      return data
    }

    return data.filter(item => {
      const timestamp = new Date(item[0]).getTime();
      return timestamp >= rangeLimits.xaxis.min! && timestamp <= rangeLimits.xaxis.max!;
    });
  }


  updateControlOptions(option: TimeRange): void {
    const rangeLimits = this.updateOptionsData[option];
    this.activeOptionButton = option;
    this.chartOptions.xaxis =
    {
      type: "datetime",
      min: rangeLimits.xaxis.min,
      max: rangeLimits.xaxis.max,
      tooltip: {
        enabled: false,
      }
    };
    this.updateChartData(this.chartOptions, this.data);
    this.cdr.detectChanges();
  }
}