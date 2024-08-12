import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { ServerStatisticsService, ServerStatus } from '../..';
import { ServerStatisticsResponse } from '../../../shared';

@Component({
  selector: 'server-slot-info-stats',
  templateUrl: './server-slot-info-stats.component.html',
  styleUrl: './server-slot-info-stats.component.scss'
})
export class ServerSlotInfoStatsComponent implements OnInit {
  @Input({ required: true }) slotKey!: string;
  serverStatus: ServerStatus = ServerStatus.NoData;
  currentServerSlotStatistics: ServerStatisticsResponse | undefined;

  get serverLastStartDateTime() {
    {
      if (!this.currentServerSlotStatistics?.serverLastStartDateTimeUTC) {
        return undefined;
      }
      return new Date(this.currentServerSlotStatistics.serverLastStartDateTimeUTC.getTime()!);
    }
  }
  get lastPulseDateTime() {
    {
      if (!this.currentServerSlotStatistics?.lastPulseDateTimeUTC) {
        return undefined;
      }
      return new Date(this.currentServerSlotStatistics.lastPulseDateTimeUTC.getTime()!);
    }
  }

  constructor(
    private readonly serverStatisticsService: ServerStatisticsService,
    private readonly cdr: ChangeDetectorRef,
  ) { }

  ngOnInit(): void {
    this.initializeStatisticsSubscription();
  }

  private initializeStatisticsSubscription(): void {
    this.serverStatisticsService.getLastServerStatistics(this.slotKey).subscribe(message => {
      this.handleStatisticsMessage(message);
    });
  }

  private handleStatisticsMessage(message: { key: string; statistics: ServerStatisticsResponse; } | null): void {
    if (!message || message.key !== this.slotKey) {
      return;
    }
    this.currentServerSlotStatistics = message.statistics;
    this.updateServerStatus();
    this.cdr.detectChanges();
  }

  private updateServerStatus(): void {
    if (this.currentServerSlotStatistics?.dataExists) {
      this.serverStatus = this.currentServerSlotStatistics.isAlive ? ServerStatus.Online : ServerStatus.Offline;
    } else {
      this.serverStatus = ServerStatus.NoData;
    }
  }
}
