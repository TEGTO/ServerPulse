import { ChangeDetectorRef, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { ServerStatisticsService } from '../..';
import { ServerLoadStatisticsResponse, ServerSlot, TimeSpan } from '../../../shared';

@Component({
  selector: 'server-slot-daily-chart',
  templateUrl: './server-slot-preview-activity-chart.component.html',
  styleUrl: './server-slot-preview-activity-chart.component.scss'
})
export class ServerSlotDailyChartComponent implements OnInit, OnDestroy {
  @Input({ required: true }) serverSlot!: ServerSlot;
  dateFrom: Date = new Date(Date.now() - this.hour);
  dateTo: Date = new Date();
  chartData: Array<[number, number]> = [];
  private statisticsSet: Map<number, number> = new Map();
  private updateTimeIntervalId?: ReturnType<typeof setInterval>;
  private isInitStatistics = true;

  get fiveMinutes() { return 5 * 60 * 1000; }
  get hour() { return 60 * 60 * 1000; }

  constructor(
    private readonly cdr: ChangeDetectorRef,
    private readonly statisticsService: ServerStatisticsService
  ) { }

  ngOnInit(): void {
    this.setUpdateTimeInterval();

    const timeSpan = new TimeSpan(0, 0, 0, this.fiveMinutes);

    this.statisticsService.getAmountStatisticsInRange(this.serverSlot.slotKey, this.dateFrom, this.dateTo, timeSpan).subscribe(statistics => {
      this.updateStatisticsSet(statistics);
      this.chartData = this.generate5MinutesTimeSeries();
      this.cdr.detectChanges();
    });

    this.initializeLoadStatisticsSubscription();
  }

  ngOnDestroy(): void {
    if (this.updateTimeIntervalId) {
      clearInterval(this.updateTimeIntervalId);
    }
  }

  private setUpdateTimeInterval() {
    this.updateTimeIntervalId = setInterval(() => {
      this.updateTime();
      this.cdr.detectChanges();
    }, this.fiveMinutes);
  }

  private updateTime() {
    const now = Date.now();
    this.dateFrom = new Date(now - this.hour);
    this.dateTo = new Date(now);
  }

  private generate5MinutesTimeSeries(): Array<[number, number]> {
    const series: Array<[number, number]> = [];
    const startTime = this.dateFrom.getTime();
    const periods = Math.ceil((this.dateTo.getTime() - startTime) / this.fiveMinutes);

    for (let i = 0; i <= periods; i++) {
      const localFrom = startTime + i * this.fiveMinutes;
      const localTo = localFrom + this.fiveMinutes;
      let count = 0;

      for (const [timestamp, amount] of this.statisticsSet) {
        if (timestamp >= localFrom && timestamp < localTo) {
          count += amount;
        }
      }

      series.push([localFrom, count]);
    }

    return series;
  }

  private initializeLoadStatisticsSubscription(): void {
    this.statisticsService.getLastServerLoadStatistics(this.serverSlot.slotKey).subscribe(message => {
      this.handleLoadStatisticsMessage(message);
    });
  }

  private handleLoadStatisticsMessage(message: { key: string; statistics: ServerLoadStatisticsResponse; } | null): void {
    if (!message) {
      return;
    }
    if (message.key !== this.serverSlot.slotKey || this.isInitStatistics) {
      this.isInitStatistics = false;
      return;
    }
    const loadTime = new Date(message.statistics.collectedDateUTC).getTime();
    this.updateTime();
    this.updateChartData(loadTime);
    this.cdr.detectChanges();
  }

  private updateChartData(loadTime: number): void {
    let isPlaceFound = false;

    for (let item of this.chartData) {
      const localFrom = item[0];
      const localTo = localFrom + this.fiveMinutes;

      if (loadTime >= localFrom && loadTime < localTo) {
        item[1]++;
        isPlaceFound = true;
        break;
      }
    }

    if (!isPlaceFound) {
      this.chartData.push([loadTime, 1]);
    }
  }

  private updateStatisticsSet(statistics: { date: Date, amountOfEvents: number }[]): void {
    for (const stat of statistics) {
      const timestamp = stat.date.getTime();
      if (!this.statisticsSet.has(timestamp)) {
        this.statisticsSet.set(timestamp, stat.amountOfEvents);
      }
    }
  }

}