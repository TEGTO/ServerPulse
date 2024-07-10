import { Injectable } from '@angular/core';
import {
  MatSnackBar
} from '@angular/material/snack-bar';
import { ErrorAnnotatedComponent } from '../..';
import { SnackbarManager } from './snackbar-manager';

@Injectable({
  providedIn: 'root'
})
export class SnackbarManagerService implements SnackbarManager {
  errorDdurationInSeconds = 5;

  constructor(private snackBar: MatSnackBar) { }

  openErrorSnackbar(errors: string[]): void {
    this.snackBar.openFromComponent(ErrorAnnotatedComponent, {
      duration: this.errorDdurationInSeconds * 1000,
      data: {
        messages: errors
      }
    });
  }
}
