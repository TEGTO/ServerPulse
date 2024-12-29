import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgApexchartsModule } from 'ng-apexcharts';
import { ActivityChartComponent, ActivityChartControlComponent } from '.';
import { PieChartComponent } from './components/pie-chart/pie-chart.component';

@NgModule({
  declarations: [
    ActivityChartComponent,
    ActivityChartControlComponent,
    PieChartComponent
  ],
  imports: [
    CommonModule,
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
export class ChartModule { }
