import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { AfterViewInit, ChangeDetectionStrategy, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Store } from '@ngrx/store';
import { filter, map, Observable, pairwise, Subject, switchMap, takeUntil, tap, throttleTime, withLatestFrom } from 'rxjs';
import { addNewLoadEvent, getSomeLoadEvents, isSelectedDateToday, selectLoadEvents, selectReadFromDate, selectSelectedDate, setReadFromDate } from '../..';
import { GetSomeMessagesRequest, LoadEvent, selectLastLifecycleStatisticsByKey, selectLastLoadEventByKey, selectLastLoadStatisticsByKey, ServerLifecycleStatistics, ServerLoadStatistics } from '../../../analyzer';
import { ServerStatus } from '../../../server-slot-shared';

@Component({
  selector: 'app-server-slot-info-stats',
  templateUrl: './server-slot-info-stats.component.html',
  styleUrl: './server-slot-info-stats.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerSlotInfoStatsComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('scroller') scroller!: CdkVirtualScrollViewport;
  @Input({ required: true }) slotKey!: string;

  lifecycleStatistics$!: Observable<ServerLifecycleStatistics>;
  loadStatistics$!: Observable<ServerLoadStatistics | undefined>;
  loadEvents$!: Observable<LoadEvent[]>;
  private readonly destroy$ = new Subject<void>();

  serverStatus: ServerStatus = ServerStatus.NoData;
  private previousDate: Date = new Date();
  readonly tableItemHeight = 65;
  private readonly tablePageAmount = 12;

  constructor(
    private readonly store: Store
  ) { }

  ngOnInit(): void {
    this.lifecycleStatistics$ = this.store.select(selectLastLifecycleStatisticsByKey(this.slotKey)).pipe(
      filter((x): x is ServerLifecycleStatistics => x !== undefined && x !== null),
      tap(statistics => {
        this.setServerStatus(statistics);
      }));

    this.loadStatistics$ = this.store.select(selectLastLoadStatisticsByKey(this.slotKey)).pipe(
      filter((x): x is ServerLoadStatistics => x !== undefined && x !== null),
    );

    this.store.select(selectLastLoadEventByKey(this.slotKey))
      .pipe(
        takeUntil(this.destroy$),
        withLatestFrom(this.store.select(selectSelectedDate)),
        tap(([event, date]) => {
          if (event && isSelectedDateToday(date)) {
            this.store.dispatch(addNewLoadEvent({ event }))
          }
        })
      )
      .subscribe();

    this.loadEvents$ = this.store.select(selectReadFromDate).pipe(
      switchMap((date) => {
        const req: GetSomeMessagesRequest = {
          key: this.slotKey,
          numberOfMessages: this.tablePageAmount,
          startDate: date,
          readNew: false,
        };
        this.store.dispatch(getSomeLoadEvents({ req: req }));
        return this.store.select(selectLoadEvents);
      }));
  }

  ngAfterViewInit(): void {
    this.monitorScrollForFetching();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private monitorScrollForFetching(): void {
    this.scroller.elementScrolled()
      .pipe(
        takeUntil(this.destroy$),
        map(() => this.scroller.measureScrollOffset('bottom')),
        pairwise(),
        filter(([previous, current]) => current < previous && current < 2 * this.tableItemHeight),
        throttleTime(200),
        withLatestFrom(this.store.select(selectLoadEvents)))
      .subscribe(([, events]) => {
        const length = events.length;
        const lastFetchedDate = new Date(length > 0 ? events[length - 1].creationDateUTC : this.previousDate);
        if (lastFetchedDate.getTime() != this.previousDate.getTime()) {
          this.previousDate = lastFetchedDate;
          this.store.dispatch(setReadFromDate({ date: lastFetchedDate }));
        }
      });
  }

  trackById(index: number, item: LoadEvent): string {
    return item.id;
  }

  private setServerStatus(statistics: ServerLifecycleStatistics): void {
    if (statistics.dataExists) {
      this.serverStatus = statistics.isAlive ? ServerStatus.Online : ServerStatus.Offline;
    } else {
      this.serverStatus = ServerStatus.NoData;
    }
  }
}
