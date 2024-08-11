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
import { RealTimeStatisticsCollector, ServerSlotComponent, ServerSlotControllerService, ServerSlotDeleteConfirmComponent, ServerSlotDialogManager, ServerSlotDialogManagerService, ServerSlotEffects, ServerSlotInfoComponent, serverSlotReducer, ServerSlotService, ServerSlotStatisticsEffects, ServerStatisticsControllerService, ServerStatisticsService, SlotBoardComponent, slotLoadStatisticsReducer, slotstatisticsReducer, StatisticsCollector } from '.';
import { AnalyticsModule } from '../analytics/analytics.module';
import { AuthenticationModule } from '../authentication/authentication.module';
import { ServerSlotDailyChartComponent } from './components/server-slot-daily-chart/server-slot-preview-activity-chart.component';
import { ServerSlotInfoChartsComponent } from './components/server-slot-info-charts/server-slot-info-charts.component';
import { ServerSlotInfoStatsComponent } from './components/server-slot-info-stats/server-slot-info-stats.component';

@NgModule({
  exports: [
    SlotBoardComponent
  ],
  declarations: [
    SlotBoardComponent,
    ServerSlotComponent,
    ServerSlotInfoComponent,
    ServerSlotDeleteConfirmComponent,
    ServerSlotDailyChartComponent,
    ServerSlotInfoChartsComponent,
    ServerSlotInfoStatsComponent
  ],
  imports: [
    AuthenticationModule,
    CommonModule,
    AnalyticsModule,
    MatButtonModule,
    MatDialogModule,
    MatInputModule,
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
    provideState({ name: "slotstatistics", reducer: slotstatisticsReducer }),
    provideState({ name: "slotloadstatistics", reducer: slotLoadStatisticsReducer }),
    provideEffects(ServerSlotEffects),
    provideEffects(ServerSlotStatisticsEffects),
  ],
})
export class ServerSlotsModule { }