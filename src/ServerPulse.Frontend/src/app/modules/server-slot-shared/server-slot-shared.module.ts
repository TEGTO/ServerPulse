import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { ServerSlotDeleteConfirmComponent, ServerSlotEffects, ServerSlotInfoDownloadComponent, ServerSlotNameChangerComponent, serverSlotReducer } from '.';

@NgModule({
  declarations: [
    ServerSlotDeleteConfirmComponent,
    ServerSlotInfoDownloadComponent,
    ServerSlotNameChangerComponent,
  ],
  imports: [
    CommonModule,
    CommonModule,
    MatButtonModule,
    MatDialogModule,
    FormsModule,
    ReactiveFormsModule,
    StoreModule.forFeature('serverslot', serverSlotReducer),
    EffectsModule.forFeature([ServerSlotEffects]),
  ],
  exports: [ServerSlotInfoDownloadComponent, ServerSlotNameChangerComponent]
})
export class ServerSlotSharedModule { }
