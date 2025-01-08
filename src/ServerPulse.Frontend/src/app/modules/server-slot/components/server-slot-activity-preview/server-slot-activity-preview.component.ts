import { ChangeDetectionStrategy, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { BehaviorSubject, interval, map, Observable, of, Subject, takeUntil } from 'rxjs';
import { addLoadEventToLoadAmountStatistics, getLoadAmountStatisticsInRange, GetLoadAmountStatisticsInRangeRequest, LoadAmountStatistics, selectLastLoadEventByKey, selectLoadAmountStatisticsByKey, startLoadStatisticsReceiving, stopLoadKeyListening } from '../../../analyzer';
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

  chartType = ActivityChartType.Box;

  private readonly dateFromSubject$ = new BehaviorSubject<Date>(this.getDateFrom());
  private readonly dateToSubject$ = new BehaviorSubject<Date>(new Date());

  dateFrom$ = this.dateFromSubject$.asObservable();
  dateTo$ = this.dateToSubject$.asObservable();
  chartData$: Observable<[number, number][]> = of([]);
  destroy$ = new Subject<void>();

  get fiveMinutes() { return 5 * 60 * 1000; }
  get hour() { return 60 * 60 * 1000; }

  constructor(
    private readonly store: Store
  ) { }

  ngOnInit(): void {
    this.setUpdateTimeInterval();

    this.setChartDataObservable();

    const timeSpan = new TimeSpan(0, 0, 0, this.fiveMinutes);
    const req: GetLoadAmountStatisticsInRangeRequest = {
      key: this.serverSlot.slotKey,
      from: this.dateFromSubject$.value,
      to: this.dateToSubject$.value,
      timeSpan: timeSpan.toString()
    }

    this.store.dispatch(getLoadAmountStatisticsInRange({ req: req }));
    this.store.dispatch(startLoadStatisticsReceiving({ key: this.serverSlot.slotKey, getInitial: false }));
  }

  ngOnDestroy(): void {
    this.store.dispatch(stopLoadKeyListening({ key: this.serverSlot.slotKey }));
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setUpdateTimeInterval(): void {
    interval(this.fiveMinutes)
      .pipe(
        takeUntil(this.destroy$)
      )
      .subscribe(
        () => this.updateTime()
      );
  }

  private setChartDataObservable(): void {
    this.chartData$ = this.store.select(selectLoadAmountStatisticsByKey(this.serverSlot.slotKey)).pipe(
      map((statistics) => {
        this.updateTime();

        const set = this.getStatisticsSet(statistics);
        const series = this.generate5MinutesTimeSeries(
          this.dateFromSubject$.value,
          this.dateToSubject$.value,
          set
        );

        return series;
      })
    );

    this.store.select(selectLastLoadEventByKey(this.serverSlot.slotKey))
      .pipe(
        takeUntil(this.destroy$)
      )
      .subscribe(event => {
        if (event) {
          this.store.dispatch(
            addLoadEventToLoadAmountStatistics({ key: this.serverSlot.slotKey, event })
          );
        }
      });
  }

  formatter(val: number): string {
    const date = new Date(val);
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');

    const nextDate = new Date(date);
    nextDate.setMinutes(date.getMinutes() + 5);
    const nextHours = nextDate.getHours().toString().padStart(2, '0');
    const nextMinutes = nextDate.getMinutes().toString().padStart(2, '0');

    return `${hours}:${minutes} - ${nextHours}:${nextMinutes}`;
  }

  private updateTime(): void {
    this.dateFromSubject$.next(this.getDateFrom());
    this.dateToSubject$.next(new Date(Date.now()));
  }

  private getDateFrom(): Date {
    return new Date(Date.now() - this.hour);
  }

  private getStatisticsSet(statistics: LoadAmountStatistics[]): Map<number, number> {
    const set: Map<number, number> = new Map<number, number>();

    statistics.forEach(stat => {
      const timestamp = stat.dateTo.getTime();
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
}
