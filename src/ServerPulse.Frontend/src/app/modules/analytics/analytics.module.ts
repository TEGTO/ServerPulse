import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { NgApexchartsModule } from 'ng-apexcharts';
import { ActivityChartDetailComponent } from './components/activity-chart-detail/activity-chart-detail.component';
import { ActivityChartComponent } from './components/activity-chart/activity-chart.component';

@NgModule({
  declarations: [
    ActivityChartComponent,
    ActivityChartDetailComponent
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
    ActivityChartDetailComponent
  ]
})
export class AnalyticsModule { }
