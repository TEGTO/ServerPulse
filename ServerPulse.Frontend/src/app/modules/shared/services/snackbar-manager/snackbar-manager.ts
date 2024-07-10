import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export abstract class SnackbarManager {
  abstract openErrorSnackbar(error: string[]): void;
}
