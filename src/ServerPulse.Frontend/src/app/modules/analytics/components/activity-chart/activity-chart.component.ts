import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnChanges, OnInit, SimpleChanges, ViewChild } from '@angular/core';
import { ChartComponent } from 'ng-apexcharts';
import { ChartOptions } from '../..';

@Component({
  selector: 'activity-chart',
  templateUrl: './activity-chart.component.html',
  styleUrls: ['./activity-chart.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivityChartComponent implements OnInit, OnChanges {
  @Input({ required: true }) chartUniqueId!: string;
  @Input({ required: true }) dateFrom!: Date;
  @Input({ required: true }) dateTo!: Date;
  @Input({ required: true }) data!: any[];
  @ViewChild('chart') chart!: ChartComponent;

  chartOptions!: Partial<ChartOptions>;

  get chartId() { return `chart-${this.chartUniqueId}`; }

  constructor(
    private readonly cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.initChartOptions();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['data']) {
      this.updateChartData();
    }
    if (changes['dateFrom'] || changes['dateTo']) {
      this.updateChartRange();
    }
  }

  private initChartOptions() {
    this.chartOptions = {
      series: [
        {
          name: "Events",
          data: this.data
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
        min: this.dateFrom.getTime(),
        max: this.dateTo.getTime(),
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

  private updateChartData() {
    if (this.chart && this.chartOptions.series) {
      this.chartOptions.series[0].data = this.data;
    }
  }

  private updateChartRange() {
    if (this.chart && this.chartOptions.xaxis) {
      this.chart.updateOptions({
        xaxis: {
          min: this.dateFrom.getTime(),
          max: this.dateTo.getTime(),
        }
      });
    }
  }
}