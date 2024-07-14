import { Component } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { ServerSlotDialogManager } from '../..';
import { RedirectorService, SnackbarManager } from '../../../shared';

@Component({
  selector: 'server-slot',
  templateUrl: './server-slot.component.html',
  styleUrl: './server-slot.component.scss'
})
export class ServerSlotComponent {
  updateRateFormControl = new FormControl('5', [Validators.required]);
  hideKey: boolean = true;

  constructor(private dialogManager: ServerSlotDialogManager, private redirector: RedirectorService, private snackBarManager: SnackbarManager) { }

  openEdit() {
    this.dialogManager.openEditServerSlotMenu();
  }
  redirectToInfo() {
    this.redirector.redirectTo("serverslot/1");
  }
  showKey() {
    this.snackBarManager.openInfoSnackbar("🔑: Some key...", 10);
  }

}
