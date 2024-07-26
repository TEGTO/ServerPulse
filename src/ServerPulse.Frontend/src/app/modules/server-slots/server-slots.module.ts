import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { RouterModule, Routes } from '@angular/router';
import { provideEffects } from '@ngrx/effects';
import { provideState, provideStore } from '@ngrx/store';
import { ServerSlotComponent, ServerSlotControllerService, ServerSlotDeleteConfirmComponent, ServerSlotDialogManager, ServerSlotDialogManagerService, ServerSlotEffects, ServerSlotInfoComponent, serverSlotReducer, ServerSlotService, SlotBoardComponent } from '.';
import { AnalyticsModule } from '../analytics/analytics.module';

const routes: Routes = [
  {
    path: "serverslot/:slotId", component: ServerSlotInfoComponent,
  }
];

@NgModule({
  exports: [
    SlotBoardComponent
  ],
  declarations: [
    SlotBoardComponent,
    ServerSlotComponent,
    ServerSlotInfoComponent,
    ServerSlotDeleteConfirmComponent
  ],
  imports: [
    CommonModule,
    AnalyticsModule,
    RouterModule.forRoot(routes),
    MatButtonModule,
    MatDialogModule,
    MatInputModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatSelectModule,
    MatMenuModule,
  ],
  providers: [
    { provide: ServerSlotDialogManager, useClass: ServerSlotDialogManagerService },
    { provide: ServerSlotService, useClass: ServerSlotControllerService },
    provideStore(),
    provideState({ name: "serverslot", reducer: serverSlotReducer }),
    provideEffects(ServerSlotEffects),
  ],
})
export class ServerSlotsModule { }