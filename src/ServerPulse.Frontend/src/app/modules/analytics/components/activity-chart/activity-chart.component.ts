import { ChangeDetectionStrategy, Component, ViewChild } from '@angular/core';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexLegend,
  ApexMarkers,
  ApexStroke,
  ApexXAxis,
  ApexYAxis,
  ChartComponent
} from "ng-apexcharts";

export type ChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  dataLabels: ApexDataLabels;
  yaxis: ApexYAxis;
  fill: ApexFill;
  legend: ApexLegend;
  stroke: ApexStroke;
  markers: ApexMarkers;
  colors: string[];
};

@Component({
  selector: 'activity-chart',
  templateUrl: './activity-chart.component.html',
  styleUrl: './activity-chart.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivityChartComponent {
  @ViewChild("chart") chart!: ChartComponent;
  public chartOptions1: Partial<ChartOptions>;
  public chartOptions2: Partial<ChartOptions>;

  constructor() {
    this.chartOptions1 = {
      series: [
        {
          name: "Pulse",
          data: this.generateDayWiseTimeSeries(
            new Date("11 Feb 2017").getTime(),
            185,
            {
              min: 0,
              max: 20
            }
          )
        },
        {
          name: "Load",
          data: this.generateDayWiseTimeSeries(
            new Date("11 Feb 2017").getTime(),
            185,
            {
              min: 0,
              max: 20
            }
          )
        }
      ],
      chart: {
        id: "chart2",
        type: "area",
        height: 230,
        stacked: false,
        toolbar: {
          autoSelected: "pan",
          show: false
        },
        animations: {
          enabled: false,
        }
      },
      colors: ['#E53F47', '#008FFB'],
      stroke: {
        width: [2, 4],
        curve: ['monotoneCubic', 'smooth']
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
      }
    };

    this.chartOptions2 = {
      series: [
        {
          name: "Pulse",
          data: this.generateDayWiseTimeSeries(
            new Date("11 Feb 2017").getTime(),
            185,
            {
              min: 0,
              max: 20
            }
          )
        },
        {
          name: "Load",
          data: this.generateDayWiseTimeSeries(
            new Date("11 Feb 2017").getTime(),
            185,
            {
              min: 0,
              max: 20
            }
          )
        }
      ],
      chart: {
        id: "chart1",
        height: 130,
        type: "area",
        stacked: true,
        brush: {
          target: "chart2",
          enabled: true
        },
        selection: {
          enabled: true,
          xaxis: {
            min: new Date("19 Jun 2017").getTime(),
            max: new Date("14 Aug 2017").getTime()
          }
        },
      },
      colors: ['#E53F47', '#008FFB'],
      stroke: {
        width: [0.5, 2],
        curve: ['monotoneCubic', 'smooth']
      },
      fill: {
        type: "gradient",
        gradient: {
          opacityFrom: 0.91,
          opacityTo: 0.1
        },
      },
      xaxis: {
        type: "datetime",
        tooltip: {
          enabled: false
        }
      },
      yaxis: {
        tickAmount: 2
      }
    };
  }

  public generateDayWiseTimeSeries(baseval: any, count: any, yrange: any) {
    var i = 0;
    var series = [];
    while (i < count) {
      var x = baseval;
      var y =
        Math.floor(Math.random() * (yrange.max - yrange.min + 1)) + yrange.min;

      series.push([x, y]);
      baseval += 86400000;
      i++;
    }
    return series;
  }
}