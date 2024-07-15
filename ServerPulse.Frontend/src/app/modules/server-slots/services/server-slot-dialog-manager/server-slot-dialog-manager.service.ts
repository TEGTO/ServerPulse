import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ServerSlotDeleteConfirmComponent } from '../..';
import { ServerSlotDialogManager } from './server-slot-dialog-manager';

@Injectable({
  providedIn: 'root'
})
export class ServerSlotDialogManagerService implements ServerSlotDialogManager {

  constructor(
    private readonly dialog: MatDialog
  ) { }

  openDeleteSlotConfirmMenu(): MatDialogRef<any> {
    var dialogRef: MatDialogRef<any, any>;
    dialogRef = this.dialog.open(ServerSlotDeleteConfirmComponent, {
      height: '200px',
      width: '450px',
    });
    return dialogRef;
  }
}
