/* eslint-disable @typescript-eslint/no-explicit-any */
import { ApexAxisChartSeries, ApexChart, ApexDataLabels, ApexFill, ApexLegend, ApexMarkers, ApexStroke, ApexTitleSubtitle, ApexTooltip, ApexXAxis, ApexYAxis } from "ng-apexcharts";

export interface ChartOptions {
    series: ApexAxisChartSeries;
    chart: ApexChart;
    title: ApexTitleSubtitle;
    xaxis: ApexXAxis;
    dataLabels: ApexDataLabels;
    yaxis: ApexYAxis;
    fill: ApexFill;
    legend: ApexLegend;
    toolbar: any;
    stroke: ApexStroke;
    markers: ApexMarkers;
    tooltip: ApexTooltip;
    colors: string[];
}