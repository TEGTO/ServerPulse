import { ScrollingModule } from '@angular/cdk/scrolling';
import { CdkTableModule } from '@angular/cdk/table';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterModule, Routes } from '@angular/router';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { ServerSlotInfoAdditionalInformationComponent, ServerSlotInfoChartsComponent, ServerSlotInfoComponent, ServerSlotInfoEffects, ServerSlotInfoStatsComponent, slotInfoStateReducer } from '.';
import { AnalyzerModule } from '../analyzer/analyzer.module';
import { ChartModule } from '../chart/chart.module';
import { ServerSlotSharedModule } from '../server-slot-shared/server-slot-shared.module';
import { LocalizedDatePipe } from '../shared';
import { CustomEventDetailsComponent } from './components/custom-event-details/custom-event-details.component';

const routes: Routes = [
  {
    path: ":id",
    component: ServerSlotInfoComponent,
  },
];

@NgModule({
  declarations: [
    ServerSlotInfoComponent,
    ServerSlotInfoStatsComponent,
    ServerSlotInfoChartsComponent,
    ServerSlotInfoAdditionalInformationComponent,
    CustomEventDetailsComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    ServerSlotSharedModule,
    AnalyzerModule,
    MatMenuModule,
    ChartModule,
    MatProgressSpinnerModule,
    LocalizedDatePipe,
    MatFormFieldModule,
    ReactiveFormsModule,
    CdkTableModule,
    ScrollingModule,
    MatDialogModule,
    StoreModule.forFeature('slotinfo', slotInfoStateReducer),
    EffectsModule.forFeature([ServerSlotInfoEffects]),
  ]
})
export class ServerSlotInfoModule { }
