import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { AuthenticatedComponent, AuthenticationService, LoginComponent, RegisterComponent } from '../..';
import { AuthenticationDialogManager } from './authentication-dialog-manager';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationDialogManagerService implements AuthenticationDialogManager {
  isAuthenticated: boolean = false;

  constructor(private authService: AuthenticationService, private dialog: MatDialog) {
    this.authService.getAuthData().subscribe(data => {
      this.isAuthenticated = data.isAuthenticated;
    })
  }

  openLoginMenu(): MatDialogRef<any, any> {
    var dialogRef: MatDialogRef<any, any>;
    if (!this.isAuthenticated) {
      dialogRef = this.dialog.open(AuthenticatedComponent, {
        height: '455px',
        width: '450px',
      });
    }
    else {
      dialogRef = this.dialog.open(LoginComponent, {
        height: '345px',
        width: '450px',
      });
    }
    return dialogRef;
  }
  openRegisterMenu(): MatDialogRef<any, any> {
    const dialogRef = this.dialog.open(RegisterComponent, {
      height: '470px',
      width: '450px',
    });
    return dialogRef;
  }
}
