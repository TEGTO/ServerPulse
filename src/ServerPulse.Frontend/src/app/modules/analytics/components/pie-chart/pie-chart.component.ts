import { Component, OnInit } from '@angular/core';
import { ApexChart, ApexLegend, ApexNonAxisChartSeries } from 'ng-apexcharts';

export type PieChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  labels: any;
  legend: ApexLegend;
};

@Component({
  selector: 'pie-chart',
  templateUrl: './pie-chart.component.html',
  styleUrl: './pie-chart.component.scss'
})
export class PieChartComponent implements OnInit {
  public chartOptions: Partial<PieChartOptions>;

  constructor() {
    this.chartOptions = {
      series: [44, 55, 13, 43, 22],
      chart: {
        width: 380,
        type: "pie"
      },
      labels: ["GET", "POST", "PUT", "PATCH", "DELETE"],
      legend: {
        position: 'bottom',
      },
    };
  }

  ngOnInit(): void {
  }
}

