import { AfterViewInit, Component, Input, OnDestroy } from '@angular/core';
import { BehaviorSubject, interval, map, of, Subject, switchMap, takeUntil } from 'rxjs';
import { ActivityChartType } from '../../../analytics';
import { ServerLoadStatisticsResponse, TimeSpan } from '../../../shared';
import { ServerStatisticsService } from '../../index';

@Component({
  selector: 'server-slot-info-charts',
  templateUrl: './server-slot-info-charts.component.html',
  styleUrls: ['./server-slot-info-charts.component.scss']
})
export class ServerSlotInfoChartsComponent implements AfterViewInit, OnDestroy {
  @Input({ required: true }) slotKey!: string;

  private cachedDateSetStatistics = new Map<number, Map<number, number>>();
  private currentSelectedDateSubject$ = new BehaviorSubject<Date>(new Date());

  private controlChartDataSubject$ = new BehaviorSubject<Array<[number, number]>>([]);
  private secondaryChartDataSubject$ = new BehaviorSubject<Array<[number, number]>>([]);

  private controlDateFromSubject$ = new BehaviorSubject<Date>(this.getStartOfDay(new Date(Date.now() - this.controlIntervalStartTime)));
  private controlDateToSubject$ = new BehaviorSubject<Date>(this.getStartOfDay(new Date()));
  private secondaryDateFromSubject$ = new BehaviorSubject<Date>(this.getAdjustedDateForSecondaryFrom());
  private secondaryDateToSubject$ = new BehaviorSubject<Date>(this.getAdjustedDateForSecondaryTo());
  private destroy$ = new Subject<void>();

  controlChartData$ = this.controlChartDataSubject$.asObservable();
  secondaryChartData$ = this.secondaryChartDataSubject$.asObservable();
  controlDateFrom$ = this.controlDateFromSubject$.asObservable();
  controlDateTo$ = this.controlDateToSubject$.asObservable();
  secondaryDateFrom$ = this.secondaryDateFromSubject$.asObservable();
  secondaryDateTo$ = this.secondaryDateToSubject$.asObservable();
  currentSelectedDate$ = this.currentSelectedDateSubject$.asObservable();
  chartType = ActivityChartType.Line;

  get controlIntervalTime() { return 24 * 60 * 60 * 1000; }
  get controlIntervalStartTime() { return 364 * this.controlIntervalTime; }
  get secondaryIntervalTime() { return 60 * 60 * 1000; }
  get secondaryIntervalStartTime() { return 24 * this.secondaryIntervalTime; }

  constructor(
    private readonly statisticsService: ServerStatisticsService
  ) { }

  ngAfterViewInit(): void {
    this.setUpdateTimeIntervals();

    this.fetchControlStatistics();

    this.handleSelectedDateUpdates();

    this.handleLastServerLoadStatistics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  controlFormatter(val: number) {
    const date = new Date(val);
    return date.toLocaleDateString(undefined, {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  }

  secondaryFormatter(val: number) {
    let date = new Date(val);
    let localHour = date.getHours();
    return `${localHour}:00 - ${localHour + 1}:00`;
  }

  private setUpdateTimeIntervals() {
    interval(this.controlIntervalTime).pipe(takeUntil(this.destroy$)).subscribe(() => this.updateControlTime());
    interval(this.secondaryIntervalTime).pipe(takeUntil(this.destroy$)).subscribe(() => this.updateSecondaryTime());
  }

  controlOnSelect($event: any): void {
    const chartData = this.controlChartDataSubject$.value;
    const dataPointIndex = $event?.dataPointIndex ?? chartData.length - 1;
    this.statisticsService.setCurrentLoadStatisticsDate(new Date(chartData[dataPointIndex][0]));
  }

  private fetchControlStatistics(): void {
    this.statisticsService.getWholeAmountStatisticsInDays(this.slotKey).pipe(
      takeUntil(this.destroy$)
    ).subscribe(statistics => {
      const set = this.getStatisticsSet(statistics);
      const series = this.generateTimeSeries(
        this.controlDateFromSubject$.value,
        this.controlDateToSubject$.value,
        this.controlIntervalTime,
        set
      );
      this.controlChartDataSubject$.next(series)
    });
  }

  private handleSelectedDateUpdates(): void {
    this.statisticsService.getCurrentLoadStatisticsDate()
      .pipe(
        takeUntil(this.destroy$)
      )
      .subscribe(date => {
        if (this.currentSelectedDateSubject$.value.getTime() !== date.getTime()) {
          this.currentSelectedDateSubject$.next(date);
        }
      });

    this.currentSelectedDate$.pipe(
      switchMap(date => this.getSecondaryChartData(date)
      ),
      takeUntil(this.destroy$)
    ).subscribe(data => { this.secondaryChartDataSubject$.next(data) });
  }

  private handleLastServerLoadStatistics(): void {
    this.statisticsService.getLastServerLoadStatistics(this.slotKey).pipe(
      takeUntil(this.destroy$)
    ).subscribe(message => {
      if (this.validateMessage(message)) {
        this.updateControlTime();
        this.updateSecondaryTime();
        const loadTime = new Date(message!.statistics.lastEvent?.creationDateUTC!).getTime();
        this.controlChartDataSubject$.next(this.addEventToChartData(loadTime, this.controlChartDataSubject$.value, this.controlIntervalTime));
        this.secondaryChartDataSubject$.next(this.addEventToChartData(loadTime, this.secondaryChartDataSubject$.value, this.secondaryIntervalTime));
      }
    });
  }

  private generateTimeSeries(fromDate: Date, toDate: Date, intervalTime: number, statistics: Map<number, number>): Array<[number, number]> {
    const series: Array<[number, number]> = [];
    const periods = Math.ceil((toDate.getTime() - fromDate.getTime()) / intervalTime);

    for (let i = 0; i <= periods; i++) {
      const localFrom = fromDate.getTime() + i * intervalTime;
      let count = 0;
      for (const [timestamp, amount] of statistics) {
        if (timestamp >= localFrom && timestamp < localFrom + intervalTime) {
          count += amount;
        }
      }
      series.push([localFrom, count]);
    }
    return series;
  }

  private addEventToChartData(loadTime: number, chartData: Array<[number, number]>, intervalTime: number): Array<[number, number]> {
    let isPlaceFound = false;
    for (const item of chartData) {
      const localFrom = item[0];
      if (loadTime >= localFrom && loadTime < localFrom + intervalTime) {
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

  private getStatisticsSet(statistics: { date: Date, amountOfEvents: number }[]): Map<number, number> {
    let set: Map<number, number> = new Map<number, number>();
    statistics.forEach(stat => {
      const timestamp = stat.date.getTime();
      if (!set.has(timestamp)) {
        set.set(timestamp, stat.amountOfEvents);
      }
    });
    return set;
  }

  private updateControlTime(): void {
    this.controlDateFromSubject$.next(this.getStartOfDay(new Date(Date.now() - this.controlIntervalStartTime)));
    this.controlDateToSubject$.next(this.getStartOfDay(new Date()));
  }

  private updateSecondaryTime(): void {
    const selectedDate = this.currentSelectedDateSubject$.value;
    if (this.isSelectedDateToday(selectedDate)) {
      this.secondaryDateFromSubject$.next(this.getAdjustedDateForSecondaryFrom());
      this.secondaryDateToSubject$.next(this.getAdjustedDateForSecondaryTo());
    } else {
      this.secondaryDateFromSubject$.next(selectedDate);
      this.secondaryDateToSubject$.next(new Date(selectedDate.getTime() + this.secondaryIntervalStartTime));
    }
  }

  private getSecondaryChartData(date: Date) {
    this.updateSecondaryTime();
    if (this.cachedDateSetStatistics.has(date.getTime()) && !this.isSelectedDateToday(date)) {
      return of(this.generateTimeSeries(
        this.secondaryDateFromSubject$.value,
        this.secondaryDateToSubject$.value,
        this.secondaryIntervalTime,
        this.cachedDateSetStatistics.get(date.getTime())!
      ));
    } else {
      return this.statisticsService.getAmountStatisticsInRange(
        this.slotKey,
        this.secondaryDateFromSubject$.value,
        this.secondaryDateToSubject$.value,
        new TimeSpan(0, 0, 0, this.secondaryIntervalTime)
      ).pipe(
        map(statistics => {
          const statisticsSet = this.getStatisticsSet(statistics);
          if (!this.isSelectedDateToday(date)) {
            this.cachedDateSetStatistics.set(date.getTime(), statisticsSet);
          }
          return this.generateTimeSeries(
            this.secondaryDateFromSubject$.value,
            this.secondaryDateToSubject$.value,
            this.secondaryIntervalTime,
            statisticsSet
          );
        })
      );
    }
  }

  private getAdjustedDateForSecondaryFrom(): Date {
    const now = new Date();
    return new Date(now.setMinutes(0, now.getSeconds(), now.getMilliseconds()) - this.secondaryIntervalStartTime + this.secondaryIntervalTime);
  }

  private getAdjustedDateForSecondaryTo(): Date {
    const now = new Date();
    return new Date(now.setMinutes(now.getMinutes(), now.getSeconds(), now.getMilliseconds()));
  }

  private isSelectedDateToday(date: Date): boolean {
    return date.setHours(0, 0, 0, 0) >= new Date().setHours(0, 0, 0, 0);
  }

  private getStartOfDay(date: Date): Date {
    return new Date(date.setHours(0, 0, 0, 0));
  }

  private validateMessage(message: { key: string; statistics: ServerLoadStatisticsResponse; } | null): boolean {
    return !!(message && message.statistics && !message.statistics.isInitial && message.key === this.slotKey);
  }
}