import { ChangeDetectorRef, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { RealTimeStatisticsCollector, ServerStatisticsService } from '../..';
import { environment } from '../../../../../environment/environment';
import { convertToServerLoadStatisticsResponse, ServerLoadResponse, ServerSlot } from '../../../shared';

@Component({
  selector: 'server-slot-daily-chart',
  templateUrl: './server-slot-daily-chart.component.html',
  styleUrl: './server-slot-daily-chart.component.scss'
})
export class ServerSlotDailyChartComponent implements OnInit, OnDestroy {
  @Input({ required: true }) serverSlot!: ServerSlot;
  dateFrom: Date = new Date(new Date().getTime() - this.hour);
  dateTo: Date = new Date();
  chartData: Array<[number, number]> | null = null;
  private serverLoadSet: Map<string, ServerLoadResponse> = new Map();
  private updateTimeIntervalId!: any;

  get hour() { return 60 * 60 * 1000; }
  get fiveMinutes() { return 5 * 60 * 1000; }
  get data() { return this.chartData || []; }

  constructor(
    private readonly cdr: ChangeDetectorRef,
    private readonly statisticsCollector: RealTimeStatisticsCollector,
    private readonly statisticsService: ServerStatisticsService
  ) { }

  ngOnInit(): void {
    this.setUpdateTimeInterval();
    this.statisticsService.getStatisticsInDateRange(this.serverSlot.slotKey, this.dateFrom, this.dateTo).subscribe(loads => {
      loads.filter(load => {
        if (!this.serverLoadSet.has(load.id)) {
          this.serverLoadSet.set(load.id, load);
        }
      });
      this.chartData = this.generate5MinutesTimeSeries();
    });
    this.initializeLoadStatisticsSubscription();
  }
  ngOnDestroy(): void {
    clearInterval(this.updateTimeIntervalId);
  }

  private setUpdateTimeInterval() {
    this.updateTimeIntervalId = setInterval(() => {
      this.updateTime();
      this.cdr.detectChanges();
    }, this.fiveMinutes);
  }
  private updateTime() {
    this.dateFrom = new Date(new Date().getTime() - this.hour);
    this.dateTo = new Date();
  }

  private generate5MinutesTimeSeries(): Array<[number, number]> {
    const series: Array<[number, number]> = [];
    const periods = (this.dateTo.getTime() - this.dateFrom.getTime()) / this.fiveMinutes;
    for (let i = 1; i <= periods + 1; i++) {
      const localFrom = this.dateFrom.getTime() + (i - 1) * this.fiveMinutes;
      const localTo = this.dateFrom.getTime() + i * this.fiveMinutes;
      let count = 0;
      this.serverLoadSet.forEach(load => {
        const loadTime = new Date(load.timestampUTC).getTime();
        if (loadTime >= localFrom && loadTime < localTo) {
          count++;
        }
      })
      series.push([localFrom, count]);
    }
    return series;
  }
  private initializeLoadStatisticsSubscription(): void {
    this.statisticsCollector.startConnection(environment.loadStatisticsHub).subscribe(() => {
      this.statisticsCollector.startListen(environment.loadStatisticsHub, this.serverSlot.slotKey);
      this.statisticsCollector.receiveStatistics(environment.loadStatisticsHub).subscribe(
        (message) => this.handleLoadStatisticsMessage(message),
        (error) => this.handleLoadStatisticsError(error)
      );
    });
  }
  private handleLoadStatisticsMessage(message: { key: string, data: string }): void {
    try {
      if (message.key !== this.serverSlot.slotKey || !this.chartData) return;

      const serverLoadStatistics = convertToServerLoadStatisticsResponse(JSON.parse(message.data));
      const loadTime = new Date(serverLoadStatistics.lastEvent.timestampUTC).getTime();

      if (loadTime >= this.dateFrom.getTime() && loadTime < this.dateTo.getTime()) {
        let isPlaceFound = false;
        const updatedChartData = [...this.chartData];

        for (let item of updatedChartData) {
          const localFrom = item[0];
          const localTo = localFrom + this.fiveMinutes;

          if (loadTime >= localFrom && loadTime < localTo) {
            item[1]++;
            isPlaceFound = true;
            break;
          }
        }

        if (!isPlaceFound) {
          updatedChartData.push([loadTime, 1]);
        }

        this.chartData = updatedChartData;
        this.updateTime();
        this.cdr.detectChanges();
      }

    } catch (error) {
      console.error('Error processing the received statistics:', error);
    }
  }
  private handleLoadStatisticsError(error: any): void {
    console.error('Error receiving statistics:', error);
  }
}
