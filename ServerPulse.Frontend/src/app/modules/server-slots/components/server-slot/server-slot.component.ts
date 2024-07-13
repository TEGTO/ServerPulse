import { Component } from '@angular/core';
import { ServerSlotDialogManager } from '../..';
import { RedirectorService } from '../../../shared';

@Component({
  selector: 'server-slot',
  templateUrl: './server-slot.component.html',
  styleUrl: './server-slot.component.scss'
})
export class ServerSlotComponent {

  constructor(private dialogManager: ServerSlotDialogManager, private redirector: RedirectorService) { }

  openEdit() {
    this.dialogManager.openEditServerSlotMenu();
  }
  redirectToInfo() {
    this.redirector.redirectTo("serverslot/1");
  }
}
