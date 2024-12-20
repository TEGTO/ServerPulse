import { ScrollingModule } from '@angular/cdk/scrolling';
import { CdkTableModule } from '@angular/cdk/table';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { CustomEventDetailsComponent, RealTimeStatisticsCollector, ServerSlotAdditionalInfromationComponent, ServerSlotDataCollector, ServerSlotDataCollectorService, ServerSlotDeleteConfirmComponent, ServerSlotInfoChartsComponent, ServerSlotInfoComponent, ServerSlotInfoStatsComponent, ServerSlotPreviewActivityChartComponent, ServerStatisticsControllerService, ServerStatisticsService, SlotInfoDownloadButtonComponent, StatisticsCollector } from '.';
import { LocalizedDatePipe } from '../shared';

@NgModule({
  exports: [
  ],
  declarations: [
    ServerSlotInfoComponent,
    ServerSlotDeleteConfirmComponent,
    ServerSlotPreviewActivityChartComponent,
    ServerSlotInfoChartsComponent,
    ServerSlotInfoStatsComponent,
    ServerSlotAdditionalInfromationComponent,
    CustomEventDetailsComponent,
    LocalizedDatePipe,
    SlotInfoDownloadButtonComponent
  ],
  imports: [
    CommonModule,
    MatButtonModule,
    MatDialogModule,
    CdkTableModule,
    MatInputModule,
    ScrollingModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatSelectModule,
    MatMenuModule,
    MatProgressSpinnerModule,
    // StoreModule.forFeature('serverslot', serverSlotReducer),
    // StoreModule.forFeature('slotstatistics', slotStatisticsReducer),
    // StoreModule.forFeature('slotloadstatistics', slotLoadStatisticsReducer),
    // StoreModule.forFeature('customstatistics', slotCustomStatisticsReducer),
    // EffectsModule.forFeature([ServerSlotEffects, ServerSlotStatisticsEffects]),
  ],
  providers: [
    { provide: RealTimeStatisticsCollector, useClass: StatisticsCollector },
    { provide: ServerStatisticsService, useClass: ServerStatisticsControllerService },
    { provide: ServerSlotDataCollector, useClass: ServerSlotDataCollectorService },
  ],
})
export class ServerSlotsModule { }