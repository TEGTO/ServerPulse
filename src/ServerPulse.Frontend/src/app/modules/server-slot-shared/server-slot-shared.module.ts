import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { ServerSlotDeleteConfirmComponent, ServerSlotEffects, ServerSlotInfoDownloadComponent, serverSlotReducer } from '.';

@NgModule({
  declarations: [
    ServerSlotDeleteConfirmComponent,
    ServerSlotInfoDownloadComponent,
  ],
  imports: [
    CommonModule,
    CommonModule,
    MatButtonModule,
    MatDialogModule,
    StoreModule.forFeature('serverslot', serverSlotReducer),
    EffectsModule.forFeature([ServerSlotEffects]),
  ],
  exports: [ServerSlotInfoDownloadComponent]
})
export class ServerSlotSharedModule { }
