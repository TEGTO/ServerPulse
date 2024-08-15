import { AfterViewInit, ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ChartComponent } from 'ng-apexcharts';
import { combineLatest, Observable } from 'rxjs';
import { ChartOptions } from '../..';

type TimeRange = "1w" | "1m" | "3m" | "6m" | "all";
@Component({
  selector: 'activity-chart-detail',
  templateUrl: './activity-chart-detail.component.html',
  styleUrl: './activity-chart-detail.component.scss'
})
export class ActivityChartDetailComponent implements OnInit, AfterViewInit {
  @Input({ required: true }) chartUniqueId!: string;
  @Input({ required: true }) controlDateFrom$!: Observable<Date>;
  @Input({ required: true }) controlDateTo$!: Observable<Date>;
  @Input({ required: true }) controlData$!: Observable<any[]>;
  @Input({ required: true }) secondaryDateFrom$!: Observable<Date>;
  @Input({ required: true }) secondaryDateTo$!: Observable<Date>;
  @Input({ required: true }) secondaryData$!: Observable<any[]>;
  @Output() onControlSelect = new EventEmitter<any>();
  @ViewChild('controlChart') controlChart!: ChartComponent;
  @ViewChild('secondaryChart') secondaryChart!: ChartComponent;

  controlChartOptions!: Partial<ChartOptions>;
  secondaryChartOptions!: Partial<ChartOptions>;
  activeOptionButton: TimeRange = "1w";
  private controlDateFrom: Date = new Date(this.currentTime.getTime() - this.week);
  private controlDateTo: Date = this.currentTime;

  get controlChartId() { return `chart1-${this.chartUniqueId}`; }
  get secondaryChartId() { return `chart2-${this.chartUniqueId}`; }
  get currentTime() { return new Date(); }
  get week() { return 7 * 24 * 60 * 60 * 1000; }

  updateOptionsData = {
    "1w": { xaxis: { min: this.currentTime.getTime() - 7 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
    "1m": { xaxis: { min: this.currentTime.getTime() - 30 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
    "3m": { xaxis: { min: this.currentTime.getTime() - 90 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
    "6m": { xaxis: { min: this.currentTime.getTime() - 180 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
    all: { xaxis: { min: undefined, max: undefined } }
  };

  constructor(
    private readonly cdr: ChangeDetectorRef,
  ) { }

  ngAfterViewInit(): void {
    combineLatest([this.secondaryDateFrom$, this.secondaryDateTo$]).subscribe(
      ([dateFrom, dateTo]) => this.updateSecondaryChartRange(dateFrom, dateTo)
    );
    this.secondaryData$.subscribe(
      (data) => this.updateChartData(this.secondaryChartOptions, data)
    );
    this.controlData$.subscribe(
      (data) => this.updateChartData(this.controlChartOptions, data)
    );
    combineLatest([this.controlDateFrom$, this.controlDateTo$]).subscribe(
      ([dateFrom, dateTo]) => this.updateControlChartRange(dateFrom, dateTo)
    );
  }

  ngOnInit(): void {
    this.initChartOptions();
  }

  initChartOptions(): void {
    this.controlChartOptions = {
      series: [
        {
          name: "Events",
          data: []
        }
      ],
      chart: {
        id: this.controlChartId,
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
        min: this.controlDateFrom.getTime(),
        max: this.controlDateTo.getTime(),
        tooltip: {
          enabled: false,
        }
      },
      tooltip:
      {
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
    this.secondaryChartOptions = {
      series: [
        {
          name: "Event",
          data: []
        }
      ],
      chart: {
        id: this.secondaryChartId,
        type: "area",
        height: 220,
        stacked: false,
        toolbar: {
          autoSelected: "pan",
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
        type: "gradient",
        gradient: {
          shadeIntensity: 1,
          inverseColors: false,
          opacityFrom: 0.45,
          opacityTo: 0.05,
          stops: [20, 100, 100, 100]
        }
      },
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
          format: 'HH'
        },
        tickAmount: 23,
        tooltip: {
          enabled: false
        }
      },
      tooltip: {
        x: {
          formatter: function (val) {
            let date = new Date(val);
            let localHour = date.getHours();
            return `${localHour}:00 - ${localHour + 1}:00`;
          }
        },
        marker: {
          show: true,
          fillColors: ['#40abfc']
        }
      },
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

  private updateControlChartRange(dateFrom: Date, dateTo: Date) {
    if (this.controlChart && this.controlChartOptions.xaxis) {
      this.updateControlOptions(this.activeOptionButton);
      this.cdr.detectChanges();
    }
  }
  private updateSecondaryChartRange(dateFrom: Date, dateTo: Date) {
    if (this.secondaryChart && this.secondaryChartOptions.xaxis) {
      this.secondaryChartOptions.xaxis =
      {
        type: "datetime",
        min: dateFrom.getTime(),
        max: dateTo.getTime(),
        labels: {
          datetimeUTC: false,
          format: 'HH'
        },
        tickAmount: 23,
        tooltip: {
          enabled: false
        }
      };
      this.cdr.detectChanges();
    }
  }

  updateControlOptions(option: TimeRange): void {
    this.activeOptionButton = option;
    this.controlChartOptions.xaxis =
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