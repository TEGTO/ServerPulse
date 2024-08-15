import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { AfterViewInit, ChangeDetectionStrategy, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { BehaviorSubject, Observable, Subject, filter, map, pairwise, shareReplay, switchMap, takeUntil, tap, throttleTime } from 'rxjs';
import { ServerStatisticsService, ServerStatus } from '../..';
import { ServerLoadResponse, ServerStatisticsResponse } from '../../../shared';

@Component({
  selector: 'server-slot-info-stats',
  templateUrl: './server-slot-info-stats.component.html',
  styleUrls: ['./server-slot-info-stats.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerSlotInfoStatsComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('scroller') scroller!: CdkVirtualScrollViewport;
  @Input({ required: true }) slotKey!: string;

  private dataSourceSubject$ = new BehaviorSubject<Array<ServerLoadResponse>>([]);
  private fetchDateSubject$ = new BehaviorSubject<Date>(new Date());
  private destroy$ = new Subject<void>();

  serverStatus$!: Observable<ServerStatus>;
  serverLastStartDateTime$!: Observable<Date | null>;
  lastPulseDateTime$!: Observable<Date | null>;
  areTableItemsLoading = false;
  currentServerSlotStatistics$!: Observable<ServerStatisticsResponse | undefined>;
  dataSource$ = this.dataSourceSubject$.asObservable();
  private fetchDate$ = this.fetchDateSubject$.asObservable();

  readonly tablePageAmount = 12;
  private readonly tableItemHeight = 55;

  constructor(
    private readonly serverStatisticsService: ServerStatisticsService,
  ) { }

  ngAfterViewInit(): void {
    this.monitorScrollForFetching();
  }

  ngOnInit(): void {
    this.initializeStatisticsSubscription();
    this.initializeFetchingTableItemsSubscription();
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
    const lastFetchedDate = length > 0 ? this.dataSourceSubject$.value[length - 1].creationDateUTC : this.fetchDateSubject$.value;
    if (lastFetchedDate !== this.fetchDateSubject$.value) {
      this.fetchDateSubject$.next(lastFetchedDate);
    }
  }

  private initializeStatisticsSubscription(): void {
    this.currentServerSlotStatistics$ = this.serverStatisticsService.getLastServerStatistics(this.slotKey)
      .pipe(
        map(message => message?.statistics),
        shareReplay(1),
      );

    this.serverStatus$ = this.currentServerSlotStatistics$.pipe(
      map(statistics => {
        if (!statistics?.dataExists) return ServerStatus.NoData;
        return statistics.isAlive ? ServerStatus.Online : ServerStatus.Offline;
      })
    );

    this.serverLastStartDateTime$ = this.currentServerSlotStatistics$.pipe(
      map(statistics => statistics?.serverLastStartDateTimeUTC ? new Date(statistics.serverLastStartDateTimeUTC) : null)
    );

    this.lastPulseDateTime$ = this.currentServerSlotStatistics$.pipe(
      map(statistics => statistics?.lastPulseDateTimeUTC ? new Date(statistics.lastPulseDateTimeUTC) : null)
    );
  }

  private initializeFetchingTableItemsSubscription(): void {
    this.fetchDate$.pipe(
      tap(() => this.areTableItemsLoading = true),
      switchMap(date => this.serverStatisticsService.getSomeLoadEventsFromDate(this.slotKey, this.tablePageAmount, date, false)),
      map(items => {
        const uniqueItems = this.getUniqueItems(this.dataSourceSubject$.value, items);
        return uniqueItems;
      }),
      takeUntil(this.destroy$)
    ).subscribe(uniqueItems => {
      this.areTableItemsLoading = false;
      this.dataSourceSubject$.next([...this.dataSourceSubject$.value, ...uniqueItems]);
    });
  }

  /**
   * Filters out duplicate items based on `creationDateUTC` and `id`.
   */
  private getUniqueItems(existingItems: ServerLoadResponse[], newItems: ServerLoadResponse[]): ServerLoadResponse[] {
    const existingSet = new Set(existingItems.map(item => `${new Date(item.creationDateUTC).getTime()}-${item.id}`));

    return newItems.filter(item => {
      const creationDate = new Date(item.creationDateUTC);
      const uniqueKey = `${creationDate.getTime()}-${item.id}`;

      if (existingSet.has(uniqueKey)) {
        return false;
      } else {
        existingSet.add(uniqueKey);
        return true;
      }
    });
  }
}