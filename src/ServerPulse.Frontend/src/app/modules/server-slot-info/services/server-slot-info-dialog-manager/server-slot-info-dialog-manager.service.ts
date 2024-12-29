import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { CustomEventDetailsComponent } from '../..';

@Injectable({
  providedIn: 'root'
})
export class ServerSlotInfoDialogManagerService {

  constructor(
    private readonly dialog: MatDialog
  ) { }

  openCustomEventDetails(serializedEvent: string): MatDialogRef<CustomEventDetailsComponent> {
    const dialogRef = this.dialog.open(CustomEventDetailsComponent, {
      height: '500px',
      width: '550px',
      data: serializedEvent
    });
    return dialogRef;
  }
}
