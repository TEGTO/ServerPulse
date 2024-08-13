import { AfterViewInit, ChangeDetectorRef, Component, Input, OnDestroy, ViewChild } from '@angular/core';
import { ServerStatisticsService } from '../..';
import { ActivityChartDetailComponent } from '../../../analytics';
import { ServerLoadStatisticsResponse, TimeSpan } from '../../../shared';

@Component({
  selector: 'server-slot-info-charts',
  templateUrl: './server-slot-info-charts.component.html',
  styleUrls: ['./server-slot-info-charts.component.scss']
})
export class ServerSlotInfoChartsComponent implements AfterViewInit, OnDestroy {
  @Input({ required: true }) slotKey!: string;
  @ViewChild('charts') charts!: ActivityChartDetailComponent;

  controlChartData: Array<[number, number]> = [];
  secondaryChartData: Array<[number, number]> = [];

  controlDateFrom: Date = this.getStartOfDay(new Date(Date.now() - this.controlIntervalStartTime));
  controlDateTo: Date = this.getStartOfDay(new Date());

  secondaryDateFrom: Date = new Date(new Date().setMinutes(0, 0, 0) - this.secondaryIntervalStartTime + this.secondaryIntervalTime);
  secondaryDateTo: Date = new Date(new Date().setMinutes(0, 0, 0) + this.secondaryIntervalTime);

  private controlStatisticsSet: Map<number, number> = new Map();
  private cachedDateSetStatistics: Map<number, Map<number, number>> = new Map();
  private currentSelectedDate: Date = new Date();
  private controlUpdateTimeIntervalId?: ReturnType<typeof setInterval>;
  private secondaryUpdateTimeIntervalId?: ReturnType<typeof setInterval>;

  get chartSecondaryDateTo() { return new Date(this.secondaryDateTo.getTime() - this.secondaryIntervalTime); }
  get selectedDate() { return this.currentSelectedDate; }
  get controlIntervalTime() { return 24 * 60 * 60 * 1000; }
  get controlIntervalStartTime() { return 364 * this.controlIntervalTime; }
  get secondaryIntervalTime() { return 60 * 60 * 1000; }
  get secondaryIntervalStartTime() { return 24 * this.secondaryIntervalTime; }

  constructor(
    private readonly cdr: ChangeDetectorRef,
    private readonly statisticsService: ServerStatisticsService
  ) { }

  ngAfterViewInit(): void {
    this.setUpdateTimeIntervals();

    this.statisticsService.getWholeAmountStatisticsInDays(this.slotKey).subscribe(statistics => {
      this.updateStatisticsSet(this.controlStatisticsSet, statistics);
      this.controlChartData = this.generateTimeSeries(this.controlDateFrom, this.controlDateTo, this.controlIntervalTime, this.controlStatisticsSet);
      this.cdr.detectChanges();
    });

    this.statisticsService.getCurrentLoadStatisticsDate().subscribe(date => {
      if (this.currentSelectedDate.getTime() !== date.getTime()) {
        this.currentSelectedDate = date;
        this.updateSecondaryTime();

        const cachedStats = this.cachedDateSetStatistics.get(this.currentSelectedDate.getTime());
        if (cachedStats && !this.isSelectedDateToday()) {
          this.secondaryChartData = this.generateTimeSeries(this.secondaryDateFrom, this.secondaryDateTo, this.secondaryIntervalTime, cachedStats);
          this.updateSecondaryChartData();
        } else {
          this.statisticsService.getAmountStatisticsInRange(this.slotKey, this.secondaryDateFrom, this.secondaryDateTo, new TimeSpan(0, 0, 0, this.secondaryIntervalTime))
            .subscribe(statistics => {
              let statisticsSet = new Map<number, number>();
              this.updateStatisticsSet(statisticsSet, statistics);
              if (!this.isSelectedDateToday()) {
                this.cachedDateSetStatistics.set(this.currentSelectedDate.getTime(), statisticsSet);
              }
              this.secondaryChartData = this.generateTimeSeries(this.secondaryDateFrom, this.secondaryDateTo, this.secondaryIntervalTime, statisticsSet);
              this.updateSecondaryChartData();
            });
        }
      }
    });

    this.initializeLoadStatisticsSubscription();
  }

  ngOnDestroy(): void {
    clearInterval(this.controlUpdateTimeIntervalId);
    clearInterval(this.secondaryUpdateTimeIntervalId);
  }

  controlOnSelect($event: any) {
    const dataPointIndex = $event?.dataPointIndex ?? this.controlChartData.length - 1;
    this.statisticsService.setCurrentLoadStatisticsDate(new Date(this.controlChartData[dataPointIndex][0]));
  }

  private setUpdateTimeIntervals() {
    this.controlUpdateTimeIntervalId = setInterval(() => this.updateControlTime(), this.controlIntervalTime);
    this.secondaryUpdateTimeIntervalId = setInterval(() => this.updateSecondaryTime(), this.secondaryIntervalTime);
  }

  private generateTimeSeries(fromDate: Date, toDate: Date, time: number, statistics: Map<number, number>): Array<[number, number]> {
    const series: Array<[number, number]> = [];
    const periods = Math.ceil((toDate.getTime() - fromDate.getTime()) / time);

    for (let i = 0; i <= periods; i++) {
      const localFrom = fromDate.getTime() + i * time;
      const localTo = localFrom + time;
      let count = 0;
      for (const [timestamp, amount] of statistics) {
        if (timestamp >= localFrom && timestamp < localTo) {
          count += amount;
        }
      }
      series.push([localFrom, count]);
    }
    return series;
  }

  private initializeLoadStatisticsSubscription(): void {
    this.statisticsService.getLastServerLoadStatistics(this.slotKey)
      .subscribe(message => this.handleLoadStatisticsMessage(message));
  }

  private handleLoadStatisticsMessage(message: { key: string; statistics: ServerLoadStatisticsResponse; } | null): void {
    if (!message || message.statistics.isInitial || message.key !== this.slotKey) {
      return;
    }

    const loadTime = new Date(message.statistics.lastEvent?.creationDateUTC!).getTime();
    this.updateControlTime();
    this.updateSecondaryTime();
    this.addEventToChartData(loadTime, this.controlChartData, this.controlIntervalTime, 1, this.charts.updateControlChartData.bind(this.charts));
    this.addEventToChartData(loadTime, this.secondaryChartData, this.secondaryIntervalTime, 2, this.charts.updateSecondaryChartData.bind(this.charts));
  }

  private addEventToChartData(loadTime: number, chartData: Array<[number, number]>, intervalTime: number, checkPlaceIndexFromEnd: number, updateChart: Function): void {
    const place = chartData[chartData.length - checkPlaceIndexFromEnd];
    const localFrom = place[0];
    const localTo = localFrom + intervalTime;

    if (loadTime >= localFrom && loadTime < localTo) {
      place[1]++;
    } else {
      chartData.push([loadTime, 1]);
    }

    updateChart();
  }

  private updateStatisticsSet(setToUpdate: Map<number, number>, statistics: { date: Date, amountOfEvents: number }[]): void {
    statistics.forEach(stat => {
      const timestamp = stat.date.getTime();
      if (!setToUpdate.has(timestamp)) {
        setToUpdate.set(timestamp, stat.amountOfEvents);
      }
    });
  }

  private updateControlTime() {
    this.controlDateFrom = this.getStartOfDay(new Date(Date.now() - this.controlIntervalStartTime));
    this.controlDateTo = this.getStartOfDay(new Date());
    this.charts?.updateControlChartRange();
  }

  private updateSecondaryTime() {
    if (this.isSelectedDateToday()) {
      this.secondaryDateFrom = new Date(new Date().setMinutes(0, new Date().getSeconds(), new Date().getMilliseconds())
        - this.secondaryIntervalStartTime + this.secondaryIntervalTime);
      this.secondaryDateTo = new Date(new Date().setMinutes(0, new Date().getSeconds(), new Date().getMilliseconds())
        + this.secondaryIntervalTime);
    } else {
      this.secondaryDateFrom = this.selectedDate;
      this.secondaryDateTo = new Date(this.selectedDate.getTime() + this.secondaryIntervalStartTime);
    }
    this.charts?.updateSecondaryChartRange();
  }

  private isSelectedDateToday(): boolean {
    return this.selectedDate.setHours(0, 0, 0, 0) >= new Date().setHours(0, 0, 0, 0);
  }

  private getStartOfDay(date: Date): Date {
    return new Date(date.setHours(0, 0, 0, 0));
  }

  private updateSecondaryChartData() {
    this.charts?.updateSecondaryChartData();
    this.cdr.detectChanges();
  }
}