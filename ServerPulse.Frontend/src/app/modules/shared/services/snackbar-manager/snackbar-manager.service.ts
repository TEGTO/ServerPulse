import { Injectable } from '@angular/core';
import {
  MatSnackBar
} from '@angular/material/snack-bar';
import { ErrorAnnotatedComponent, InfoAnnotatedComponent } from '../..';
import { SnackbarManager } from './snackbar-manager';

@Injectable({
  providedIn: 'root'
})
export class SnackbarManagerService implements SnackbarManager {
  errorDurationInSeconds = 5;

  constructor(private snackBar: MatSnackBar) { }

  openInfoSnackbar(message: string, durationInSeconds: number): void {
    this.snackBar.openFromComponent(InfoAnnotatedComponent, {
      duration: durationInSeconds * 1000,
      data: {
        message: message
      }
    });
  }
  openErrorSnackbar(errors: string[]): void {
    this.snackBar.openFromComponent(ErrorAnnotatedComponent, {
      duration: this.errorDurationInSeconds * 1000,
      data: {
        messages: errors
      }
    });
  }
}
