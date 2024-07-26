import { Component } from '@angular/core';
import { AuthenticationDialogManager } from '../..';

@Component({
  selector: 'auth-unauthenticated',
  templateUrl: './unauthenticated.component.html',
  styleUrl: './unauthenticated.component.scss'
})
export class UnauthenticatedComponent {

  constructor(
    private readonly authDialogManager: AuthenticationDialogManager
  ) { }

  openLoginMenu() {
    const dialogRef = this.authDialogManager.openLoginMenu();
  }
}
