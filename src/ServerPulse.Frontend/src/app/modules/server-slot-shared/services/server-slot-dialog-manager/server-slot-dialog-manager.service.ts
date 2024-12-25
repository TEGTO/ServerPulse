/* eslint-disable @typescript-eslint/no-explicit-any */
import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ServerSlotDeleteConfirmComponent } from '../..';

@Injectable({
  providedIn: 'root'
})
export class ServerSlotDialogManagerService {

  constructor(
    private readonly dialog: MatDialog
  ) { }

  openDeleteSlotConfirmMenu(): MatDialogRef<any> {
    const dialogRef = this.dialog.open(ServerSlotDeleteConfirmComponent, {
      height: '200px',
      width: '450px',
    });
    return dialogRef;
  }
}
