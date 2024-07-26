import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ActivityChartComponent } from './components/activity-chart/activity-chart.component';

@NgModule({
  declarations: [
    ActivityChartComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    ActivityChartComponent
  ]
})
export class AnalyticsModule { }
