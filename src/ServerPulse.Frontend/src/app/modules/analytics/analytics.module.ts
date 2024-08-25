import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { NgApexchartsModule } from 'ng-apexcharts';
import { ActivityChartControlComponent } from './components/activity-chart-control/activity-chart-control.component';
import { ActivityChartComponent } from './components/activity-chart/activity-chart.component';
import { PieChartComponent } from './components/pie-chart/pie-chart.component';

@NgModule({
  declarations: [
    ActivityChartComponent,
    ActivityChartControlComponent,
    PieChartComponent
  ],
  imports: [
    CommonModule,
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    NgApexchartsModule,
  ],
  exports: [
    ActivityChartComponent,
    ActivityChartControlComponent,
    PieChartComponent
  ]
})
export class AnalyticsModule { }
