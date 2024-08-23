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
import { provideEffects } from '@ngrx/effects';
import { provideState, provideStore } from '@ngrx/store';
import { CustomEventDetailsComponent, RealTimeStatisticsCollector, ServerSlotAdditionalInfromationComponent, ServerSlotComponent, ServerSlotControllerService, ServerSlotDeleteConfirmComponent, ServerSlotDialogManager, ServerSlotDialogManagerService, ServerSlotEffects, ServerSlotInfoChartsComponent, ServerSlotInfoComponent, ServerSlotInfoStatsComponent, ServerSlotPreviewActivityChartComponent, serverSlotReducer, ServerSlotService, ServerSlotStatisticsEffects, ServerStatisticsControllerService, ServerStatisticsService, SlotBoardComponent, slotCustomStatisticsReducer, slotLoadStatisticsReducer, slotStatisticsReducer, StatisticsCollector } from '.';
import { AnalyticsModule } from '../analytics/analytics.module';
import { AuthenticationModule } from '../authentication/authentication.module';
import { LocalizedDatePipe } from '../shared';

@NgModule({
  exports: [
    SlotBoardComponent
  ],
  declarations: [
    SlotBoardComponent,
    ServerSlotComponent,
    ServerSlotInfoComponent,
    ServerSlotDeleteConfirmComponent,
    ServerSlotPreviewActivityChartComponent,
    ServerSlotInfoChartsComponent,
    ServerSlotInfoStatsComponent,
    ServerSlotAdditionalInfromationComponent,
    CustomEventDetailsComponent,
    LocalizedDatePipe
  ],
  imports: [
    AuthenticationModule,
    CommonModule,
    AnalyticsModule,
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
    MatProgressSpinnerModule
  ],
  providers: [
    { provide: ServerSlotDialogManager, useClass: ServerSlotDialogManagerService },
    { provide: ServerSlotService, useClass: ServerSlotControllerService },
    { provide: RealTimeStatisticsCollector, useClass: StatisticsCollector },
    { provide: ServerStatisticsService, useClass: ServerStatisticsControllerService },
    provideStore(),
    provideState({ name: "serverslot", reducer: serverSlotReducer }),
    provideState({ name: "slotstatistics", reducer: slotStatisticsReducer }),
    provideState({ name: "slotloadstatistics", reducer: slotLoadStatisticsReducer }),
    provideState({ name: "customstatistics", reducer: slotCustomStatisticsReducer }),
    provideEffects(ServerSlotEffects),
    provideEffects(ServerSlotStatisticsEffects),
  ],
})
export class ServerSlotsModule { }