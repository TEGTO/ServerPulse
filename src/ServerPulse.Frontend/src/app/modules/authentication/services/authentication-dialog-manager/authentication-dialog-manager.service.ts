/* eslint-disable @typescript-eslint/no-explicit-any */
import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { AuthenticatedComponent, LoginComponent, RegisterComponent } from '../..';
import { AuthenticationDialogManager } from './authentication-dialog-manager';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationDialogManagerService implements AuthenticationDialogManager {
  constructor(
    private readonly dialog: MatDialog,
  ) {
  }

  closeAll(): void {
    this.dialog.closeAll();
  }

  openLoginMenu(): MatDialogRef<any> {
    const dialogRef = this.dialog.open(LoginComponent, {
      maxHeight: '455px',
      width: '500px',
    });
    return dialogRef;
  }

  openAuthenticatedMenu(): MatDialogRef<any> {
    const dialogRef = this.dialog.open(AuthenticatedComponent, {
      maxHeight: '440px',
      width: '450px',
    });
    return dialogRef;
  }

  openRegisterMenu(): MatDialogRef<any> {
    const dialogRef = this.dialog.open(RegisterComponent, {
      maxHeight: '455px',
      width: '500px',
    });
    return dialogRef;
  }
}
