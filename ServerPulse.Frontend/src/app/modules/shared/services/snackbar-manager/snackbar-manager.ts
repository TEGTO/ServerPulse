import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export abstract class SnackbarManager {
  abstract openInfoSnackbar(message: string, durationInSeconds: number): void;
  abstract openErrorSnackbar(error: string[]): void;
}
