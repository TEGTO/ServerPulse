import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ServerSlotEditComponent } from '../..';
import { ServerSlotDialogManager } from './server-slot-dialog-manager';

@Injectable({
  providedIn: 'root'
})
export class ServerSlotDialogManagerService implements ServerSlotDialogManager {

  constructor(private dialog: MatDialog) {
  }

  openEditServerSlotMenu(): MatDialogRef<any> {
    var dialogRef: MatDialogRef<any, any>;
    dialogRef = this.dialog.open(ServerSlotEditComponent, {
      height: '230px',
      width: '450px',
    });
    return dialogRef;
  }
}
