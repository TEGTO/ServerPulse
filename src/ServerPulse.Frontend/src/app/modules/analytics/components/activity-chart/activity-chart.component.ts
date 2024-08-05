import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { ChartOptions } from '../..';

@Component({
  selector: 'activity-chart',
  templateUrl: './activity-chart.component.html',
  styleUrls: ['./activity-chart.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivityChartComponent implements OnInit {
  @Input({ required: true }) public chartUniqueId!: string;
  public chartOptions!: Partial<ChartOptions>;

  constructor() { }

  ngOnInit(): void {
    const currentTime = new Date();
    const hourAgo = new Date(currentTime.getTime() - 1 * 60 * 60 * 1000);
    const chartId = `chart-${this.chartUniqueId}`;

    this.chartOptions = {
      series: [
        {
          name: "Events",
          data: this.generate5MinutesTimeSeries(
            hourAgo,
            12,
            {
              min: 0,
              max: 100
            }
          ),
        }
      ],
      chart: {
        id: chartId,
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
        min: hourAgo.getTime(),
        max: currentTime.getTime(),
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
            let nextMinutes = (date.getMinutes() + 5).toString().padStart(2, '0');
            return `${hours}:${minutes} - ${hours}:${nextMinutes}`;
          }
        },
        marker: {
          show: true,
          fillColors: ['#40abfc']
        }
      },
    };
  }

  public generate5MinutesTimeSeries(baseval: Date, count: number, yrange: { min: number, max: number }) {
    const series = [];
    for (let i = 0; i < count; i++) {
      const x = baseval.getTime() + i * 5 * 60 * 1000;
      const y = Math.floor(Math.random() * (yrange.max - yrange.min + 1)) + yrange.min;
      series.push([x, y]);
    }
    return series;
  }
}