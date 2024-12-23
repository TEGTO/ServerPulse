import { AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { ChartComponent } from 'ng-apexcharts';
import { combineLatest, Observable, Subject, takeUntil } from 'rxjs';
import { ChartOptions } from '../../index';

type TimeRange = "1w" | "1m" | "3m" | "6m" | "all";
@Component({
  selector: 'activity-chart-control',
  templateUrl: './activity-chart-control.component.html',
  styleUrl: './activity-chart-control.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivityChartControlComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input({ required: true }) uniqueId!: string;
  @Input({ required: true }) dateFrom$!: Observable<Date>;
  @Input({ required: true }) dateTo$!: Observable<Date>;
  @Input({ required: true }) data$!: Observable<any[]>;
  @Output() onControlSelect = new EventEmitter<any>();
  @Input({ required: true }) formatter?: (val: number, opts?: any) => string;
  @ViewChild('chart') chart!: ChartComponent;

  chartOptions!: Partial<ChartOptions>;
  activeOptionButton: TimeRange = "1w";
  private dateFrom: Date = new Date(this.currentTime.getTime() - this.week);
  private dateTo: Date = this.currentTime;
  private destroy$ = new Subject<void>();

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
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  ngAfterViewInit(): void {
    this.data$
      .pipe(takeUntil(this.destroy$))
      .subscribe((data) => this.updateChartData(this.chartOptions, data));
    combineLatest([this.dateFrom$, this.dateTo$])
      .pipe(takeUntil(this.destroy$))
      .subscribe(([dateFrom, dateTo]) => this.updateChartRange(dateFrom, dateTo));
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
            this.onControlSelect.emit(opts);
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

  private updateChartData(chartOptions: Partial<ChartOptions>, data: any) {
    if (chartOptions != null && chartOptions.series != null) {
      chartOptions.series =
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
      this.updateControlOptions(this.activeOptionButton);
      this.cdr.detectChanges();
    }
  }
  updateControlOptions(option: TimeRange): void {
    this.activeOptionButton = option;
    this.chartOptions.xaxis =
    {
      type: "datetime",
      min: this.updateOptionsData[option].xaxis.min,
      max: this.updateOptionsData[option].xaxis.max,
      tooltip: {
        enabled: false,
      }
    };
    this.cdr.detectChanges();
  }
}