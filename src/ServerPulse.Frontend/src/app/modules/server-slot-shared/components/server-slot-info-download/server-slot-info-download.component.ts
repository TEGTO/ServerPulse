import { Component, Input } from '@angular/core';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-server-slot-info-download',
  templateUrl: './server-slot-info-download.component.html',
  styleUrl: './server-slot-info-download.component.scss'
})
export class ServerSlotInfoDownloadComponent {
  @Input({ required: true }) slotKey!: string;


  constructor(private readonly store: Store) { }

  downloadData() {

  }
}
