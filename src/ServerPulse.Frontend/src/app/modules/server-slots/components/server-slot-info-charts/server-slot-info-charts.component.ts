import { AfterViewInit, Component, Input, OnDestroy } from '@angular/core';
import { BehaviorSubject, interval, map, of, Subject, switchMap, takeUntil } from 'rxjs';
import { ServerStatisticsService } from '../..';
import { ServerLoadStatisticsResponse, TimeSpan } from '../../../shared';

@Component({
  selector: 'server-slot-info-charts',
  templateUrl: './server-slot-info-charts.component.html',
  styleUrls: ['./server-slot-info-charts.component.scss']
})
export class ServerSlotInfoChartsComponent implements AfterViewInit, OnDestroy {
  @Input({ required: true }) slotKey!: string;

  private cachedDateSetStatistics: Map<number, Map<number, number>> = new Map();
  private controlStatisticsSetSubject$ = new BehaviorSubject<Map<number, number>>(new Map());
  private currentSelectedDateSubject$ = new BehaviorSubject<Date>(new Date());

  private controlChartDataSubject$ = new BehaviorSubject<Array<[number, number]>>([]);
  private secondaryChartDataSubject$ = new BehaviorSubject<Array<[number, number]>>([]);

  private controlDateFromSubject$ = new BehaviorSubject<Date>(this.getStartOfDay(new Date(Date.now() - this.controlIntervalStartTime)));
  private controlDateToSubject$ = new BehaviorSubject<Date>(this.getStartOfDay(new Date()));
  private secondaryDateFromSubject$ = new BehaviorSubject<Date>(new Date(new Date().setMinutes(0, 0, 0) - this.secondaryIntervalStartTime + this.secondaryIntervalTime));
  private secondaryDateToSubject$ = new BehaviorSubject<Date>(new Date(new Date().setMinutes(0, 0, 0) + this.secondaryIntervalTime));
  private destroy$ = new Subject<void>();

  controlChartData$ = this.controlChartDataSubject$.asObservable();
  secondaryChartData$ = this.secondaryChartDataSubject$.asObservable();
  controlDateFrom$ = this.controlDateFromSubject$.asObservable();
  controlDateTo$ = this.controlDateToSubject$.asObservable();
  secondaryDateFrom$ = this.secondaryDateFromSubject$.asObservable();
  secondaryDateTo$ = this.secondaryDateToSubject$.asObservable();
  controlStatisticsSet$ = this.controlStatisticsSetSubject$.asObservable();
  currentSelectedDate$ = this.currentSelectedDateSubject$.asObservable();

  get controlIntervalTime() { return 24 * 60 * 60 * 1000; }
  get controlIntervalStartTime() { return 364 * this.controlIntervalTime; }
  get secondaryIntervalTime() { return 60 * 60 * 1000; }
  get secondaryIntervalStartTime() { return 24 * this.secondaryIntervalTime; }

  constructor(
    private readonly statisticsService: ServerStatisticsService
  ) { }

  ngAfterViewInit(): void {
    this.setUpdateTimeIntervals();

    this.statisticsService.getWholeAmountStatisticsInDays(this.slotKey)
      .pipe(takeUntil(this.destroy$))
      .subscribe(statistics => {
        const controlSet = this.controlStatisticsSetSubject$.value;
        this.controlStatisticsSetSubject$.next(this.updateStatisticsSet(controlSet, statistics));
      });

    this.controlStatisticsSet$.pipe(
      map(statisticsSet => {
        return this.generateTimeSeries(this.controlDateFromSubject$.value, this.controlDateToSubject$.value, this.controlIntervalTime, statisticsSet);
      }),
      takeUntil(this.destroy$)
    ).subscribe(chartData => {
      this.controlChartDataSubject$.next(chartData);
    });

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
      switchMap(date => {
        this.updateSecondaryTime();
        if (this.cachedDateSetStatistics.has(date.getTime()) && !this.isSelectedDateToday(date)) {
          return of(this.generateTimeSeries(this.secondaryDateFromSubject$.value, this.secondaryDateToSubject$.value, this.secondaryIntervalTime, this.cachedDateSetStatistics.get(date.getTime())!));
        } else {
          return this.statisticsService.getAmountStatisticsInRange(this.slotKey, this.secondaryDateFromSubject$.value, this.secondaryDateToSubject$.value, new TimeSpan(0, 0, 0, this.secondaryIntervalTime))
            .pipe(
              map(statistics => {
                const statisticsSet = this.updateStatisticsSet(new Map(), statistics);
                if (!this.isSelectedDateToday(date)) {
                  this.cachedDateSetStatistics.set(date.getTime(), statisticsSet);
                }
                return this.generateTimeSeries(this.secondaryDateFromSubject$.value, this.secondaryDateToSubject$.value, this.secondaryIntervalTime, statisticsSet);
              })
            );
        }
      }),
      takeUntil(this.destroy$)
    ).subscribe(data => {
      this.secondaryChartDataSubject$.next(data)
    })

    this.statisticsService.getLastServerLoadStatistics(this.slotKey)
      .pipe(takeUntil(this.destroy$))
      .subscribe(message => {
        if (this.validateMessage(message)) {
          this.updateControlTime();
          this.updateSecondaryTime();
          const loadTime = new Date(message!.statistics.lastEvent?.creationDateUTC!).getTime();
          this.controlChartDataSubject$.next(this.addEventToChartData(loadTime, this.controlChartDataSubject$.value, this.controlIntervalTime));
          this.secondaryChartDataSubject$.next(this.addEventToChartData(loadTime, this.secondaryChartDataSubject$.value, this.secondaryIntervalTime));
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setUpdateTimeIntervals() {
    interval(this.controlIntervalTime).pipe(takeUntil(this.destroy$)).subscribe(() => this.updateControlTime());
    interval(this.secondaryIntervalTime).pipe(takeUntil(this.destroy$)).subscribe(() => this.updateSecondaryTime());
  }

  controlOnSelect($event: any) {
    const chartData = this.controlChartDataSubject$.value;
    const dataPointIndex = $event?.dataPointIndex ?? chartData.length - 1;
    this.statisticsService.setCurrentLoadStatisticsDate(new Date(chartData[dataPointIndex][0]));
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

  private addEventToChartData(loadTime: number, chartData: Array<[number, number]>, intervalTime: number) {
    let isPlaceFound = false;
    for (let item of chartData) {
      const localFrom = item[0];
      const localTo = localFrom + intervalTime;

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
    return message && !message.statistics.isInitial && message.key === this.slotKey;
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

  private updateControlTime() {
    this.controlDateFromSubject$.next(this.getStartOfDay(new Date(Date.now() - this.controlIntervalStartTime)));
    this.controlDateToSubject$.next(this.getStartOfDay(new Date()));
  }

  private updateSecondaryTime() {
    if (this.isSelectedDateToday(this.currentSelectedDateSubject$.value)) {
      this.secondaryDateFromSubject$.next(new Date(new Date().setMinutes(0, new Date().getSeconds(), new Date().getMilliseconds())
        - this.secondaryIntervalStartTime + this.secondaryIntervalTime));
      this.secondaryDateToSubject$.next(new Date(new Date().setMinutes(0, new Date().getSeconds(), new Date().getMilliseconds())
        + this.secondaryIntervalTime));
    } else {
      this.secondaryDateFromSubject$.next(this.currentSelectedDateSubject$.value);
      this.secondaryDateToSubject$.next(new Date(this.currentSelectedDateSubject$.value.getTime() + this.secondaryIntervalStartTime));
    }
  }

  private isSelectedDateToday(date: Date): boolean {
    return date.setHours(0, 0, 0, 0) >= new Date().setHours(0, 0, 0, 0);
  }

  private getStartOfDay(date: Date): Date {
    return new Date(date.setHours(0, 0, 0, 0));
  }
}