import { ChangeDetectorRef, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { RealTimeStatisticsCollector, ServerStatisticsService } from '../..';

@Component({
  selector: 'server-slot-info-charts',
  templateUrl: './server-slot-info-charts.component.html',
  styleUrl: './server-slot-info-charts.component.scss'
})
export class ServerSlotInfoChartsComponent implements OnInit, OnDestroy {
  @Input({ required: true }) slotKey!: string;

  constructor(
    private readonly cdr: ChangeDetectorRef,
    private readonly statisticsCollector: RealTimeStatisticsCollector,
    private readonly statisticsService: ServerStatisticsService
  ) { }

  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }
  ngOnDestroy(): void {
    throw new Error('Method not implemented.');
  }
}
