import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { ChartComponent } from 'ng-apexcharts';
import { ChartOptions } from '../..';

type TimeRange = "1w" | "1m" | "3m" | "6m" | "all";
@Component({
  selector: 'activity-chart-detail',
  templateUrl: './activity-chart-detail.component.html',
  styleUrl: './activity-chart-detail.component.scss'
})
export class ActivityChartDetailComponent implements OnInit {
  @ViewChild("controlChart", { static: false }) controlChart!: ChartComponent;
  @Input({ required: true }) chartUniqueId!: string;
  controlChartOptions!: Partial<ChartOptions>;
  dailyChartOptions!: Partial<ChartOptions>;
  activeOptionButton = "1w";
  private readonly currentTime: Date = new Date();

  updateOptionsData = {
    "1w": { xaxis: { min: this.currentTime.getTime() - 7 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
    "1m": { xaxis: { min: this.currentTime.getTime() - 30 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
    "3m": { xaxis: { min: this.currentTime.getTime() - 90 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
    "6m": { xaxis: { min: this.currentTime.getTime() - 180 * 24 * 60 * 60 * 1000, max: this.currentTime.getTime() } },
    all: { xaxis: { min: undefined, max: undefined } }
  };

  constructor() { }
  ngOnInit(): void {
    let controlChartId = `chart2-${this.chartUniqueId}`;
    let dailyChartId = `chart1-${this.chartUniqueId}`;

    const yearAgo = new Date(this.currentTime.getTime() - 365 * 24 * 60 * 60 * 1000);
    const currentHour = new Date(this.currentTime.setMinutes(0, 0, 0));
    const oneDayAgo = new Date(currentHour.getTime() - 24 * 60 * 60 * 1000);

    this.controlChartOptions = {
      series: [
        {
          name: "Events",
          data: this.generateDayWiseTimeSeries(
            yearAgo,
            366,
            {
              min: 0,
              max: 100
            }
          )
        }
      ],
      chart: {
        id: controlChartId,
        height: 260,
        type: "bar",
        stacked: true,
        selection: {
          enabled: true,
          xaxis: {
            min: this.currentTime.getTime() - 6 * 24 * 60 * 60 * 1000,
            max: this.currentTime.getTime()
          }
        },
        zoom: {
          type: "x",
          enabled: true,
          autoScaleYaxis: true
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
            console.log(chartContext, opts);
          }
        }
      },
      dataLabels: {
        enabled: false
      },
      stroke: {
        width: [2],
        curve: ['smooth']
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
      xaxis: {
        type: "datetime",
        min: this.currentTime.getTime() - 6 * 24 * 60 * 60 * 1000,
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
    this.dailyChartOptions = {
      series: [
        {
          name: "Event",
          data: this.generateHourlyTimeSeries(
            oneDayAgo,
            25,
            {
              min: 0,
              max: 20
            }
          )
        }
      ],
      chart: {
        id: dailyChartId,
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
        min: oneDayAgo.getTime(),
        max: currentHour.getTime(),
        labels: {
          datetimeUTC: false,
          format: 'HH'
        },
        tickAmount: 25,
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

  public generateHourlyTimeSeries(baseval: any, count: any, yrange: any) {
    const series = [];
    for (let i = 0; i < count; i++) {
      const x = baseval.getTime() + i * 3600 * 1000;
      const y = Math.floor(Math.random() * (yrange.max - yrange.min + 1)) + yrange.min;
      series.push([x, y]);
    }
    return series;
  }
  public generateDayWiseTimeSeries(baseval: any, count: any, yrange: any) {
    const series = [];
    for (let i = 0; i < count; i++) {
      const x = baseval.getTime() + i * 24 * 3600 * 1000;
      const y = Math.floor(Math.random() * (yrange.max - yrange.min + 1)) + yrange.min;
      series.push([x, y]);
    }
    return series;
  }
  public updateOptions(option: TimeRange): void {
    this.activeOptionButton = option;
    this.controlChart.updateOptions(this.updateOptionsData[option], false, true, true);
  }
}
