/* eslint-disable @typescript-eslint/no-explicit-any */
import { ApexChart, ApexLegend, ApexNonAxisChartSeries } from "ng-apexcharts";

export interface PieChartOptions {
    series: ApexNonAxisChartSeries;
    chart: ApexChart;
    labels: any;
    legend: ApexLegend;
}