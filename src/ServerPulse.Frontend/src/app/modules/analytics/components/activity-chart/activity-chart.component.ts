import { AfterViewInit, ChangeDetectorRef, Component, Input, OnInit, ViewChild } from '@angular/core';
import { ChartComponent } from 'ng-apexcharts';
import { combineLatest, Observable } from 'rxjs';
import { ChartOptions } from '../..';

@Component({
  selector: 'activity-chart',
  templateUrl: './activity-chart.component.html',
  styleUrls: ['./activity-chart.component.scss'],
})
export class ActivityChartComponent implements OnInit, AfterViewInit {
  @Input({ required: true }) chartUniqueId!: string;
  @Input({ required: true }) dateFrom$!: Observable<Date>;
  @Input({ required: true }) dateTo$!: Observable<Date>;
  @Input({ required: true }) data$!: Observable<any[]>;
  @ViewChild('chart') chart!: ChartComponent;

  chartOptions!: Partial<ChartOptions>;

  get chartId() { return `chart-${this.chartUniqueId}`; }

  constructor(
    private readonly cdr: ChangeDetectorRef,
  ) { }

  ngAfterViewInit(): void {
    combineLatest([this.dateFrom$, this.dateTo$]).subscribe(
      ([dateFrom, dateTo]) => this.updateChartRange(dateFrom, dateTo)
    );
    this.data$.subscribe(
      (data) => this.updateChartData(data)
    );
  }

  ngOnInit(): void {
    this.initChartOptions();
  }

  private initChartOptions() {
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
        toolbar: {
          autoSelected: "pan",
          show: false
        },
        animations: {
          enabled: true,
        }
      },
      fill: {
        type: "gradient",
        gradient: {
          shade: "light",
          type: "horizontal",
          shadeIntensity: 0.25,
          inverseColors: true,
          opacityFrom: 1,
          opacityTo: 1,
          stops: [50, 0, 100, 100]
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
          formatter: function (val) {
            let date = new Date(val);
            let hours = date.getHours().toString().padStart(2, '0');
            let minutes = date.getMinutes().toString().padStart(2, '0');
            let nextDate = new Date(date);
            nextDate.setMinutes(date.getMinutes() + 5);
            let nextHours = nextDate.getHours().toString().padStart(2, '0');
            let nextMinutes = nextDate.getMinutes().toString().padStart(2, '0');
            return `${hours}:${minutes} - ${nextHours}:${nextMinutes}`;
          }
        },
        marker: {
          show: true,
          fillColors: ['#40abfc']
        }
      },
    };
  }

  private updateChartData(data: any[]) {
    if (this.chart && this.chartOptions.series) {
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
}