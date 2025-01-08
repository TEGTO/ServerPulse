/* eslint-disable @typescript-eslint/no-explicit-any */
import { AfterViewInit, ChangeDetectionStrategy, Component, Input, OnDestroy } from '@angular/core';
import { Store } from '@ngrx/store';
import { BehaviorSubject, filter, interval, Subject, takeUntil, tap } from 'rxjs';
import { checkIfLoadEventAlreadyExistsById, getDailyLoadAmountStatistics, getLoadAmountStatisticsInRange, isSelectedDateToday, selectLoadAmountStatistics, selectSecondaryLoadAmountStatistics, selectSelectedDate, setSelectedDate } from '../..';
import { GetLoadAmountStatisticsInRangeRequest, LoadAmountStatistics, selectLastLoadEventByKey } from '../../../analyzer';
import { ActivityChartType } from '../../../chart';
import { TimeSpan } from '../../../shared';

@Component({
  selector: 'app-server-slot-info-charts',
  templateUrl: './server-slot-info-charts.component.html',
  styleUrl: './server-slot-info-charts.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerSlotInfoChartsComponent implements AfterViewInit, OnDestroy {
  @Input({ required: true }) slotKey!: string;

  chartType = ActivityChartType.Line;
  currentSelectedDate: Date = new Date();

  private readonly controlChartDataSubject$ = new BehaviorSubject<[number, number][]>([]);
  private readonly secondaryChartDataSubject$ = new BehaviorSubject<[number, number][]>([]);
  private readonly controlDateFromSubject$ = new BehaviorSubject<Date>(this.getStartOfDay(new Date(Date.now() - this.controlIntervalStartTime)));
  private readonly controlDateToSubject$ = new BehaviorSubject<Date>(this.getStartOfDay(new Date()));
  private readonly secondaryDateFromSubject$ = new BehaviorSubject<Date>(this.getAdjustedDateForSecondaryFrom());
  private readonly secondaryDateToSubject$ = new BehaviorSubject<Date>(this.getAdjustedDateForSecondaryTo());
  private readonly destroy$ = new Subject<void>();

  controlChartData$ = this.controlChartDataSubject$.asObservable();
  secondaryChartData$ = this.secondaryChartDataSubject$.asObservable();
  controlDateFrom$ = this.controlDateFromSubject$.asObservable();
  controlDateTo$ = this.controlDateToSubject$.asObservable();
  secondaryDateFrom$ = this.secondaryDateFromSubject$.asObservable();
  secondaryDateTo$ = this.secondaryDateToSubject$.asObservable();

  get controlIntervalTime() { return 24 * 60 * 60 * 1000; }
  get controlIntervalStartTime() { return 364 * this.controlIntervalTime; }
  get secondaryIntervalTime() { return 60 * 60 * 1000; }
  get secondaryIntervalStartTime() { return 24 * this.secondaryIntervalTime; }

  constructor(
    private readonly store: Store
  ) { }

  ngAfterViewInit(): void {
    this.setUpdateTimeIntervals();

    this.store.dispatch(getDailyLoadAmountStatistics({ key: this.slotKey }));
    this.fetchControlStatistics();

    this.handleSelectedDateUpdates();

    this.handleLastServerLoadStatistics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setUpdateTimeIntervals(): void {
    interval(this.controlIntervalTime).pipe(takeUntil(this.destroy$)).subscribe(() => this.updateControlTime());
    interval(this.secondaryIntervalTime).pipe(takeUntil(this.destroy$)).subscribe(() => this.updateSecondaryTime());
  }

  private fetchControlStatistics(): void {
    this.store.select(selectLoadAmountStatistics)
      .pipe(
        takeUntil(this.destroy$),
        tap(statistics => {
          const set = this.getStatisticsSet(statistics);
          const series = this.generateTimeSeriesForControl(
            this.controlDateFromSubject$.value,
            this.controlDateToSubject$.value,
            this.controlIntervalTime,
            set
          );
          this.controlChartDataSubject$.next(series)
        })
      )
      .subscribe();
  }

  private handleSelectedDateUpdates(): void {
    this.store.select(selectSelectedDate)
      .pipe(
        takeUntil(this.destroy$),
        tap(date => {
          if (this.currentSelectedDate.getTime() !== date?.getTime()) {
            this.currentSelectedDate = date;

            this.updateSecondaryTime();

            const req: GetLoadAmountStatisticsInRangeRequest =
            {
              key: this.slotKey,
              from: this.secondaryDateFromSubject$.value,
              to: this.secondaryDateToSubject$.value,
              timeSpan: new TimeSpan(0, 0, 0, this.secondaryIntervalTime).toString()
            }

            this.store.dispatch(getLoadAmountStatisticsInRange({ req }))
          }
        })
      )
      .subscribe();

    this.store.select(selectSecondaryLoadAmountStatistics)
      .pipe(
        takeUntil(this.destroy$),
        tap(statistics => {
          const statisticsSet = this.getStatisticsSet(statistics);
          this.secondaryChartDataSubject$.next(
            this.generateTimeSeries(
              this.secondaryDateFromSubject$.value,
              this.secondaryDateToSubject$.value,
              this.secondaryIntervalTime,
              statisticsSet
            ));
        }))
      .subscribe();
  }

  private handleLastServerLoadStatistics(): void {
    this.store.select(selectLastLoadEventByKey(this.slotKey))
      .pipe(
        takeUntil(this.destroy$),
        filter(event => {
          return event !== null && !checkIfLoadEventAlreadyExistsById(event?.id)
        }),
        tap(event => {
          if (event) {
            this.updateControlTime();
            this.updateSecondaryTime();
            const loadTime = new Date(event.creationDateUTC).getTime();
            this.controlChartDataSubject$.next(this.addEventToChartData(loadTime, this.controlChartDataSubject$.value, this.controlIntervalTime));
            this.secondaryChartDataSubject$.next(this.addEventToChartData(loadTime, this.secondaryChartDataSubject$.value, this.secondaryIntervalTime));
          }
        }))
      .subscribe();
  }

  private updateControlTime(): void {
    this.controlDateFromSubject$.next(this.getStartOfDay(new Date(Date.now() - this.controlIntervalStartTime)));
    this.controlDateToSubject$.next(this.getStartOfDay(new Date()));
  }

  private updateSecondaryTime(): void {
    const selectedDate = this.currentSelectedDate;
    if (isSelectedDateToday(selectedDate)) {
      this.secondaryDateFromSubject$.next(this.getAdjustedDateForSecondaryFrom());
      this.secondaryDateToSubject$.next(this.getAdjustedDateForSecondaryTo());
    } else {
      this.secondaryDateFromSubject$.next(selectedDate);
      this.secondaryDateToSubject$.next(new Date(selectedDate.getTime() + this.secondaryIntervalStartTime));
    }
  }

  private generateTimeSeries(fromDate: Date, toDate: Date, intervalTime: number, statistics: Map<number, number>): [number, number][] {
    const periods = Math.ceil((toDate.getTime() - fromDate.getTime()) / intervalTime);

    // Generate the series atomically
    const series = Array.from({ length: periods + 1 }, (_, i) => {
      const localFrom = fromDate.getTime() + i * intervalTime;
      let count = 0;

      for (const [timestamp, amount] of statistics) {
        if (timestamp >= localFrom && timestamp < localFrom + intervalTime) {
          count += amount;
        }
      }

      return [localFrom, count] as [number, number];
    });

    return series;
  }

  private generateTimeSeriesForControl(fromDate: Date, toDate: Date, intervalTime: number, statistics: Map<number, number>): [number, number][] {
    const periods = Math.ceil((toDate.getTime() - fromDate.getTime()) / intervalTime);

    // Generate the series atomically
    const series = Array.from({ length: periods + 1 }, (_, i) => {
      const index = periods - i;
      const localTo = fromDate.getTime() + index * intervalTime;
      let count = 0;

      for (const [timestamp, amount] of statistics) {
        if (timestamp >= localTo - intervalTime && timestamp <= localTo) {
          count += amount;
          console.log("timestamp: " + new Date(timestamp));
          console.log("Local from: " + new Date(localTo - intervalTime));
          console.log("Local to: " + new Date(localTo));
        }
      }

      return [localTo, count] as [number, number];
    });

    return series;
  }

  controlFormatter(val: number): string {
    const dateFrom = new Date(val);

    const strFrom = dateFrom.toLocaleDateString(undefined, {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
    return `${strFrom}`;
  }

  secondaryFormatter(val: number): string {
    const date = new Date(val);
    const localHour = date.getHours();
    return `${localHour}:00 - ${localHour + 1}:00`;
  }

  controlOnSelect($event: any): void {
    const chartData = this.controlChartDataSubject$.value;
    const dataPointIndex = $event?.dataPointIndex ?? chartData.length - 1;

    const selectedDate = new Date(chartData[dataPointIndex][0]);

    let readFromDate = new Date(selectedDate);
    if (isSelectedDateToday(selectedDate)) {
      readFromDate = new Date();
    }
    else {
      readFromDate.setHours(23, 59, 59, 999);
    }

    this.store.dispatch(setSelectedDate({ date: selectedDate, readFromDate: readFromDate }));
  }

  private addEventToChartData(loadTime: number, chartData: [number, number][], intervalTime: number): [number, number][] {
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

  private getStatisticsSet(statistics: LoadAmountStatistics[]): Map<number, number> {
    const set: Map<number, number> = new Map<number, number>();

    statistics.forEach(stat => {
      const timestamp = stat.dateFrom.getTime();
      if (!set.has(timestamp)) {
        set.set(timestamp, stat.amountOfEvents);
      }
    });

    return set;
  }

  private getAdjustedDateForSecondaryFrom(): Date {
    const now = new Date();
    return new Date(now.setMinutes(0, now.getSeconds(), now.getMilliseconds()) - this.secondaryIntervalStartTime + this.secondaryIntervalTime);
  }

  private getAdjustedDateForSecondaryTo(): Date {
    const now = new Date();
    return new Date(now.setMinutes(now.getMinutes(), now.getSeconds(), now.getMilliseconds()));
  }

  private getStartOfDay(date: Date): Date {
    return new Date(date.setHours(0, 0, 0, 0));
  }
}