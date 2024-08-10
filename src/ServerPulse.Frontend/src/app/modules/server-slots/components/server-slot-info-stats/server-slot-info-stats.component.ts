import { Component, Input } from '@angular/core';
import { ServerStatus } from '../..';

@Component({
  selector: 'server-slot-info-stats',
  templateUrl: './server-slot-info-stats.component.html',
  styleUrl: './server-slot-info-stats.component.scss'
})
export class ServerSlotInfoStatsComponent {
  @Input({ required: true }) serverStatus!: ServerStatus;
}
