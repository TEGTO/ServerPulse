import { ChangeDetectionStrategy, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { BehaviorSubject, interval, Observable, of, Subject, takeUntil } from 'rxjs';
import { LoadAmountStatisticsResponse, ServerLoadStatisticsResponse } from '../../../analyzer';
import { ActivityChartType } from '../../../chart';
import { ServerSlot } from '../../../server-slot-shared';
import { TimeSpan } from '../../../shared';

@Component({
  selector: 'app-server-slot-activity-preview',
  templateUrl: './server-slot-activity-preview.component.html',
  styleUrl: './server-slot-activity-preview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerSlotActivityPreviewComponent implements OnInit, OnDestroy {
  @Input({ required: true }) serverSlot!: ServerSlot;

  private readonly dateFromSubject$ = new BehaviorSubject<Date>(this.getDateFrom());
  private readonly dateToSubject$ = new BehaviorSubject<Date>(new Date());
  private readonly destroy$ = new Subject<void>();

  dateFrom$ = this.dateFromSubject$.asObservable();
  dateTo$ = this.dateToSubject$.asObservable();
  chartData$: Observable<[number, number][]> = of([]);
  chartType = ActivityChartType.Box;

  get fiveMinutes() { return 5 * 60 * 1000; }
  get hour() { return 60 * 60 * 1000; }

  constructor(
    // private readonly statisticsService: ServerStatisticsService
  ) { }

  ngOnInit(): void {
    this.setUpdateTimeInterval();

    const timeSpan = new TimeSpan(0, 0, 0, this.fiveMinutes);
    let series: [number, number][];

    // const statistics$ = this.statisticsService.getLoadAmountStatisticsInRange(this.serverSlot.slotKey, this.dateFromSubject$.value, this.dateToSubject$.value, timeSpan).pipe(
    //   shareReplay(1)
    // );

    // this.chartData$ = this.statisticsService.getLastServerLoadStatistics(this.serverSlot.slotKey).pipe(
    //   switchMap(lastLoadStatistics =>
    //     statistics$.pipe(
    //       map(statistics => {
    //         if (!series) {
    //           const set = this.getStatisticsSet(statistics);
    //           series = this.generate5MinutesTimeSeries(this.dateFromSubject$.value, this.dateToSubject$.value, set);
    //         }
    //         if (this.validateMessage(lastLoadStatistics)) {
    //           this.updateTime();
    //           const loadTime = new Date(lastLoadStatistics!.statistics.lastEvent?.creationDateUTC!).getTime();
    //           series = this.addEventToChartData(series, loadTime);
    //         }
    //         return series;
    //       })
    //     )
    //   ),
    // );
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  formatter(val: number) {
    const date = new Date(val);
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    const nextDate = new Date(date);
    nextDate.setMinutes(date.getMinutes() + 5);
    const nextHours = nextDate.getHours().toString().padStart(2, '0');
    const nextMinutes = nextDate.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes} - ${nextHours}:${nextMinutes}`;
  }

  private setUpdateTimeInterval(): void {
    interval(this.fiveMinutes).pipe(takeUntil(this.destroy$)).subscribe(() => this.updateTime());
  }

  private updateTime(): void {
    this.dateFromSubject$.next(this.getDateFrom());
    this.dateToSubject$.next(new Date(Date.now()));
  }

  private getDateFrom(): Date {
    return new Date(Date.now() - this.hour);
  }

  private getStatisticsSet(statistics: LoadAmountStatisticsResponse[]) {
    const set: Map<number, number> = new Map<number, number>();
    statistics.forEach(stat => {
      const timestamp = stat.dateFrom.getTime();
      if (!set.has(timestamp)) {
        set.set(timestamp, stat.amountOfEvents);
      }
    });
    return set;
  }

  private generate5MinutesTimeSeries(dateFrom: Date, dateTo: Date, statisticsSet: Map<number, number>): [number, number][] {
    const series: [number, number][] = [];
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

  private addEventToChartData(chartData: [number, number][], loadTime: number): [number, number][] {
    let isPlaceFound = false;
    for (const item of chartData) {
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
    return message?.statistics /*&& !message.statistics.isInitial*/ && message.key === this.serverSlot.slotKey;
  }
}
