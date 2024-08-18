import { AfterViewInit, Component, Input, OnDestroy } from '@angular/core';
import { BehaviorSubject, interval, map, Subject, takeUntil } from 'rxjs';
import { ServerStatisticsService } from '../..';
import { ServerLoadStatisticsResponse, ServerSlot, TimeSpan } from '../../../shared';

@Component({
  selector: 'server-slot-daily-chart',
  templateUrl: './server-slot-preview-activity-chart.component.html',
  styleUrl: './server-slot-preview-activity-chart.component.scss'
})
export class ServerSlotPreviewActivityChartComponent implements AfterViewInit, OnDestroy {
  @Input({ required: true }) serverSlot!: ServerSlot;

  private dateFromSubject$ = new BehaviorSubject<Date>(this.getDateFrom());
  private dateToSubject$ = new BehaviorSubject<Date>(new Date());
  private statisticsSetSubject$ = new BehaviorSubject<Map<number, number>>(new Map());
  private chartDataSubject$ = new BehaviorSubject<Array<[number, number]>>([]);
  private destroy$ = new Subject<void>();

  dateFrom$ = this.dateFromSubject$.asObservable();
  dateTo$ = this.dateToSubject$.asObservable();
  statisticsSet$ = this.statisticsSetSubject$.asObservable();
  chartData$ = this.chartDataSubject$.asObservable();

  get fiveMinutes() { return 5 * 60 * 1000; }
  get hour() { return 60 * 60 * 1000; }

  constructor(
    private readonly statisticsService: ServerStatisticsService
  ) { }

  ngAfterViewInit(): void {
    this.setUpdateTimeInterval();

    const timeSpan = new TimeSpan(0, 0, 0, this.fiveMinutes);

    this.statisticsService.getAmountStatisticsInRange(this.serverSlot.slotKey, this.dateFromSubject$.value, this.dateToSubject$.value, timeSpan)
      .pipe(takeUntil(this.destroy$))
      .subscribe(statistics => {
        const set = this.statisticsSetSubject$.value;
        this.statisticsSetSubject$.next(this.updateStatisticsSet(set, statistics));
      });

    this.statisticsSet$.pipe(
      map(statisticsSet => {
        this.updateTime();
        return this.generate5MinutesTimeSeries(this.dateFromSubject$.value, this.dateToSubject$.value, statisticsSet);
      }),
      takeUntil(this.destroy$)
    ).subscribe(chartData => {
      this.chartDataSubject$.next(chartData);
    });

    this.statisticsService.getLastServerLoadStatistics(this.serverSlot.slotKey).pipe(
      takeUntil(this.destroy$)
    ).subscribe(message => {
      if (this.validateMessage(message)) {
        const loadTime = new Date(message!.statistics.lastEvent?.creationDateUTC!).getTime();
        this.chartDataSubject$.next(this.addEventToChartData(loadTime));
      }
    });

    this.chartData$.pipe(map(() => {
      this.updateTime();
    }));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setUpdateTimeInterval(): void {
    interval(this.fiveMinutes).pipe(
      takeUntil(this.destroy$)
    ).subscribe(() => this.updateTime());
  }

  private updateTime(): void {
    const now = Date.now();
    this.dateFromSubject$.next(this.getDateFrom());
    this.dateToSubject$.next(new Date(now));
  }

  private getDateFrom(): Date {
    return new Date(Date.now() - this.hour);
  }

  private updateStatisticsSet(set: Map<number, number>, statistics: { date: Date, amountOfEvents: number }[]) {
    for (const stat of statistics) {
      const timestamp = stat.date.getTime();
      if (!set.has(timestamp)) {
        set.set(timestamp, stat.amountOfEvents);
      }
    }
    return set;
  }

  private generate5MinutesTimeSeries(dateFrom: Date, dateTo: Date, statisticsSet: Map<number, number>): Array<[number, number]> {
    const series: Array<[number, number]> = [];
    const startTime = dateFrom.getTime();
    const periods = Math.ceil((dateTo.getTime() - startTime) / this.fiveMinutes);

    for (let i = 0; i <= periods; i++) {
      const localFrom = startTime + i * this.fiveMinutes;
      const localTo = localFrom + this.fiveMinutes;
      let count = 0;

      for (const [timestamp, amount] of statisticsSet) {
        if (timestamp >= localFrom && timestamp < localTo) {
          count += amount;
        }
      }

      series.push([localFrom, count]);
    }
    return series;
  }

  private addEventToChartData(loadTime: number) {
    let chartData = this.chartDataSubject$.value;
    let isPlaceFound = false;
    for (let item of chartData) {
      const localFrom = item[0];
      const localTo = localFrom + this.fiveMinutes;

      if (loadTime >= localFrom && loadTime < localTo) {
        item[1]++;
        isPlaceFound = true;
        break;
      }
    }
    if (!isPlaceFound) {
      chartData.push([loadTime, 1]);
    }
    return chartData;
  }

  private validateMessage(message: { key: string; statistics: ServerLoadStatisticsResponse; } | null) {
    return message && message.statistics && !message.statistics.isInitial && message.key === this.serverSlot.slotKey;
  }
}