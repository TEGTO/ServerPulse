import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { AfterViewInit, ChangeDetectionStrategy, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { BehaviorSubject, Observable, Subject, filter, map, of, pairwise, shareReplay, switchMap, takeUntil, tap, throttleTime } from 'rxjs';
import { LoadEventResponse, ServerLoadStatisticsResponse, ServerStatisticsResponse, getUniqueItems } from '../../../../shared';
import { ServerStatisticsService, ServerStatus } from '../../../index';

@Component({
  selector: 'server-slot-info-stats',
  templateUrl: './server-slot-info-stats.component.html',
  styleUrls: ['./server-slot-info-stats.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerSlotInfoStatsComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('scroller') scroller!: CdkVirtualScrollViewport;
  @Input({ required: true }) slotKey!: string;

  private dataSourceSubject$ = new BehaviorSubject<LoadEventResponse[]>([]);
  private fetchDateSubject$ = new BehaviorSubject<Date>(new Date());
  private areTableItemsLoadingSubject$ = new BehaviorSubject<boolean>(false);

  dataSource$: Observable<LoadEventResponse[]> = of([]);
  areTableItemsLoading$ = this.areTableItemsLoadingSubject$.asObservable();
  serverStatus$!: Observable<ServerStatus>;
  serverLastStartDateTime$!: Observable<Date | null>;
  lastPulseDateTime$!: Observable<Date | null>;
  currentServerSlotStatistics$!: Observable<ServerStatisticsResponse | undefined>;
  loadStatistics$!: Observable<ServerLoadStatisticsResponse | undefined>;
  private fetchDate$ = this.fetchDateSubject$.asObservable();
  private destroy$ = new Subject<void>();

  readonly tableItemHeight = 65;
  private readonly tablePageAmount = 12;

  constructor(
    private readonly statisticsService: ServerStatisticsService,
  ) { }

  ngOnInit(): void {
    this.initializeStatisticsSubscription();
    this.initializeFetchingTableItemsSubscription();
    this.initializeLoadStatisticsSubscription();
  }

  ngAfterViewInit(): void {
    this.monitorScrollForFetching();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private monitorScrollForFetching(): void {
    this.scroller.elementScrolled().pipe(
      map(() => this.scroller.measureScrollOffset('bottom')),
      pairwise(),
      filter(([previous, current]) => current < previous && current < 2 * this.tableItemHeight),
      throttleTime(200),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.triggerDataFetch();
    });
  }

  private triggerDataFetch(): void {
    const length = this.dataSourceSubject$.value.length;
    const lastFetchedDate = new Date(length > 0 ? this.dataSourceSubject$.value[length - 1].creationDateUTC : this.fetchDateSubject$.value);
    if (lastFetchedDate.getTime() != this.fetchDateSubject$.value.getTime()) {
      this.fetchDateSubject$.next(lastFetchedDate);
    }
  }

  private initializeStatisticsSubscription(): void {
    this.currentServerSlotStatistics$ = this.statisticsService.getLastServerStatistics(this.slotKey).pipe(
      map(message => message?.statistics),
    );

    this.serverStatus$ = this.currentServerSlotStatistics$.pipe(
      map(statistics => statistics?.dataExists ? (statistics.isAlive ? ServerStatus.Online : ServerStatus.Offline) : ServerStatus.NoData),
    );

    this.serverLastStartDateTime$ = this.currentServerSlotStatistics$.pipe(
      map(statistics => statistics?.serverLastStartDateTimeUTC ? new Date(statistics.serverLastStartDateTimeUTC) : null),
    );

    this.lastPulseDateTime$ = this.currentServerSlotStatistics$.pipe(
      map(statistics => statistics?.lastPulseDateTimeUTC ? new Date(statistics.lastPulseDateTimeUTC) : null),
    );
  }

  private initializeLoadStatisticsSubscription(): void {
    this.statisticsService.getCurrentLoadStatisticsDate().pipe(
      takeUntil(this.destroy$)
    ).subscribe(date => {
      if (this.fetchDateSubject$.value.getTime() !== date.getTime()) {
        this.dataSourceSubject$.next([]);
        const newDate = this.isSelectedDateToday(date) ? new Date() : new Date(date.setHours(23, 59, 50, 999));
        this.fetchDateSubject$.next(newDate);
      }
    });
  }

  private initializeFetchingTableItemsSubscription(): void {
    this.dataSource$ = this.fetchDate$.pipe(
      tap(() => this.areTableItemsLoadingSubject$.next(true)),
      switchMap(date => this.statisticsService.getSomeLoadEventsFromDate(this.slotKey, this.tablePageAmount, date, false).pipe(
        shareReplay(1),
        switchMap(events => this.statisticsService.getLastServerLoadStatistics(this.slotKey).pipe(
          tap(lastStatistics => this.loadStatistics$ = of(lastStatistics?.statistics)),
          map(lastStatistics => {
            let uniqueItems = getUniqueItems(this.dataSourceSubject$.value, events) as LoadEventResponse[];
            this.dataSourceSubject$.next([...this.dataSourceSubject$.value, ...uniqueItems]);
            if (this.validateMessage(lastStatistics)) {
              let newEvent = lastStatistics?.statistics.lastEvent;
              this.dataSourceSubject$.next([newEvent!, ...this.dataSourceSubject$.value]);
            }
            this.areTableItemsLoadingSubject$.next(false)
            return this.dataSourceSubject$.value;
          })
        )
        )),
      ));
  }

  trackById(index: number, item: LoadEventResponse): string {
    return item.id;
  }
  private isSelectedDateToday(date: Date): boolean {
    return date.setHours(0, 0, 0, 0) >= new Date().setHours(0, 0, 0, 0);
  }
  private validateMessage(message: { key: string; statistics: ServerLoadStatisticsResponse; } | null) {
    return message
      && !message.statistics.isInitial
      && message.key === this.slotKey
      && message?.statistics?.lastEvent
      && this.isSelectedDateToday(this.fetchDateSubject$.value);
  }
}