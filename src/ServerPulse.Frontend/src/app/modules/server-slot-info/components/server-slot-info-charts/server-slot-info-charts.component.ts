/* eslint-disable @typescript-eslint/no-explicit-any */
import { AfterViewInit, ChangeDetectionStrategy, Component, Input, OnDestroy } from '@angular/core';
import { Store } from '@ngrx/store';
import { BehaviorSubject, interval, Subject, takeUntil, tap } from 'rxjs';
import { addNewLoadEvent, getDailyLoadAmountStatistics, getLoadAmountStatisticsInRange, isSelectedDateToday, selectLoadAmountStatistics, selectSecondaryLoadAmountStatistics, selectSelectedDate, setLoadStatisticsInterval, setSecondaryLoadStatisticsInterval, setSelectedDate } from '../..';
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
    this.store.dispatch(setLoadStatisticsInterval({ interval: this.controlIntervalTime }));
    this.store.dispatch(setSecondaryLoadStatisticsInterval({ interval: this.secondaryIntervalTime }));
    this.fetchControlStatistics();

    this.handleSelectedDateUpdates();

    this.handleNewEvent();
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
          const fromDate = this.controlDateFromSubject$.value;
          const toDate = this.controlDateToSubject$.value;

          const fullYearSeries = this.generateTimeSeries(fromDate, toDate, statistics, this.controlIntervalTime);

          this.controlChartDataSubject$.next(fullYearSeries);
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
          const fromDate = this.secondaryDateFromSubject$.value;
          const toDate = this.secondaryDateToSubject$.value;

          const secondaryTimSeries = this.generateTimeSeries(fromDate, toDate, statistics, this.secondaryIntervalTime);
          this.secondaryChartDataSubject$.next(secondaryTimSeries);
        }))
      .subscribe();
  }

  private handleNewEvent(): void {
    this.store.select(selectLastLoadEventByKey(this.slotKey))
      .pipe(
        takeUntil(this.destroy$),
        tap((event) => {
          if (event) {
            this.store.dispatch(addNewLoadEvent({ event }));
          }
        })
      )
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

  private generateTimeSeries(
    fromDate: Date,
    toDate: Date,
    statistics: LoadAmountStatistics[],
    intervalTime: number
  ): [number, number][] {
    const set: Map<number, number> = new Map<number, number>();

    statistics.forEach(stat => {
      const timestamp = stat.dateFrom.getTime();
      if (!set.has(timestamp)) {
        set.set(timestamp, stat.amountOfEvents);
      }
    });

    const periods = Math.ceil((toDate.getTime() - fromDate.getTime()) / intervalTime);

    return Array.from({ length: periods + 1 }, (_, i) => {
      const localFrom = fromDate.getTime() + i * intervalTime;
      let count = 0;

      for (const [timestamp, amount] of set) {
        if (timestamp >= localFrom && timestamp < localFrom + intervalTime) {
          count += amount;
        }
      }

      return [localFrom, count] as [number, number];
    });
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

  controlOnSelect(value: any): void {
    const selectedDate = new Date(value);

    let readFromDate = new Date(selectedDate);
    if (isSelectedDateToday(selectedDate)) {
      readFromDate = new Date();
    }
    else {
      readFromDate.setHours(23, 59, 59, 999);
    }

    this.store.dispatch(setSelectedDate({ date: selectedDate, readFromDate: readFromDate }));
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