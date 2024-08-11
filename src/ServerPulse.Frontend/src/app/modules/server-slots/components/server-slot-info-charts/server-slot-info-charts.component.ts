import { ChangeDetectorRef, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ServerStatisticsService } from '../..';
import { ActivityChartDetailComponent } from '../../../analytics';
import { ServerLoadStatisticsResponse } from '../../../shared';

@Component({
  selector: 'server-slot-info-charts',
  templateUrl: './server-slot-info-charts.component.html',
  styleUrl: './server-slot-info-charts.component.scss'
})
export class ServerSlotInfoChartsComponent implements OnInit, OnDestroy {
  @Input({ required: true }) slotKey!: string;
  @ViewChild('charts') charts!: ActivityChartDetailComponent;
  controlChartData: Array<[number, number]> = [];
  controlDateFrom: Date = new Date(new Date(Date.now() - this.year).setHours(0, 0, 0, 0));
  controlDateTo: Date = new Date(new Date().setHours(0, 0, 0, 0));
  private controlStatisticsSet: Map<number, number> = new Map();
  private isInitStatistics = true;

  get day() { return 24 * 60 * 60 * 1000; }
  get year() { return 364 * this.day; }

  constructor(
    private readonly cdr: ChangeDetectorRef,
    private readonly statisticsService: ServerStatisticsService
  ) { }

  ngOnInit(): void {
    // this.setUpdateTimeIntervals();

    this.statisticsService.getWholeAmountStatisticsInDays(this.slotKey).subscribe(statistics => {
      this.updateStatisticsSet(this.controlStatisticsSet, statistics);
      this.controlChartData = this.generateControlTimeSeries();
      this.cdr.detectChanges();
    });
    this.initializeLoadStatisticsSubscription();
  }

  ngOnDestroy(): void {
    throw new Error('Method not implemented.');
  }

  private generateControlTimeSeries(): Array<[number, number]> {
    const series: Array<[number, number]> = [];
    const startTime = this.controlDateFrom.getTime();
    const periods = Math.ceil((this.controlDateTo.getTime() - startTime) / this.day);

    for (let i = 0; i <= periods; i++) {
      const localFrom = startTime + i * this.day;
      const localTo = localFrom + this.day;
      let count = 0;

      for (const [timestamp, amount] of this.controlStatisticsSet) {
        if (timestamp >= localFrom && timestamp < localTo) {
          count += amount;
        }
      }

      series.push([localFrom, count]);
    }

    return series;
  }
  private initializeLoadStatisticsSubscription(): void {
    this.statisticsService.getLastServerLoadStatistics(this.slotKey).subscribe(message => {
      this.handleLoadStatisticsMessage(message);
    });
  }

  private handleLoadStatisticsMessage(message: { key: string; statistics: ServerLoadStatisticsResponse; } | null): void {
    if (!message) {
      return;
    }
    if (message.key !== this.slotKey || this.isInitStatistics) {
      this.isInitStatistics = false;
      return;
    }
    const loadTime = new Date(message.statistics.collectedDateUTC).getTime();
    this.updateTime();
    this.updateControlChartData(loadTime);
    this.cdr.detectChanges();
  }

  private updateStatisticsSet(setToUpdate: Map<number, number>, statistics: { date: Date, amountOfEvents: number }[]): void {
    for (const stat of statistics) {
      const timestamp = stat.date.getTime();
      if (!setToUpdate.has(timestamp)) {
        setToUpdate.set(timestamp, stat.amountOfEvents);
      }
    }
  }
  private updateTime() {
    this.controlDateFrom = new Date(new Date(Date.now() - this.year).setHours(0, 0, 0, 0));
    this.controlDateTo = new Date(new Date().setHours(0, 0, 0, 0));
    this.charts.updateControlChartRange();
  }

  private updateControlChartData(loadTime: number): void {
    let isPlaceFound = false;

    let last = this.controlChartData[this.controlChartData.length - 1];
    const localFrom = last[0];
    const localTo = localFrom + this.day;

    if (loadTime >= localFrom && loadTime < localTo) {
      last[1]++;
      isPlaceFound = true;
    }
    if (!isPlaceFound) {
      this.controlChartData.push([loadTime, 1]);
    }
    this.charts.updateControlChartData();
  }
}
