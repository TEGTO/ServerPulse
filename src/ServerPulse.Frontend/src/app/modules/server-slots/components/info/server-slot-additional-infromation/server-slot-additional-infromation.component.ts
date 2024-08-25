import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { AfterViewInit, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { BehaviorSubject, filter, map, Observable, of, pairwise, shareReplay, Subject, switchMap, takeUntil, tap, throttleTime } from 'rxjs';
import { CustomEventResponse, CustomEventStatisticsResponse, getUniqueItems, ServerLoadStatisticsResponse } from '../../../../shared';
import { ServerSlotDialogManager, ServerStatisticsService } from '../../../index';

@Component({
  selector: 'server-slot-additional-info',
  templateUrl: './server-slot-additional-infromation.component.html',
  styleUrl: './server-slot-additional-infromation.component.scss'
})
export class ServerSlotAdditionalInfromationComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('scroller') scroller!: CdkVirtualScrollViewport;
  @Input({ required: true }) slotKey!: string;

  private dataSourceSubject$ = new BehaviorSubject<CustomEventResponse[]>([]);
  private fetchDateSubject$ = new BehaviorSubject<Date>(new Date());
  private areTableItemsLoadingSubject$ = new BehaviorSubject<boolean>(false);

  chartData$: Observable<Map<string, number>> = of(new Map<string, number>);
  dataSource$: Observable<CustomEventResponse[]> = of([]);
  areTableItemsLoading$ = this.areTableItemsLoadingSubject$.asObservable();
  private fetchDate$ = this.fetchDateSubject$.asObservable();
  private destroy$ = new Subject<void>();

  readonly tableItemHeight = 65;
  private readonly tablePageAmount = 12;

  private get currentDate() { return new Date(); }

  constructor(
    private readonly dialogManager: ServerSlotDialogManager,
    private readonly statisticsService: ServerStatisticsService
  ) { }

  ngOnInit(): void {
    this.initializeDataChartFetching();
    this.initializeFetchingTableItemsSubscription();
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
    const lastFetchedDate = new Date(length > 0 ? this.dataSourceSubject$.value[length - 1].creationDateUTC : this.currentDate);
    if (lastFetchedDate.getTime() != this.fetchDateSubject$.value.getTime()) {
      this.fetchDateSubject$.next(lastFetchedDate);
    }
  }

  private initializeDataChartFetching() {
    this.chartData$ = this.statisticsService.getLastServerLoadStatistics(this.slotKey).pipe(
      map(lastLoadStatistics => this.generateChartSeries(lastLoadStatistics?.statistics!)),
    );
  }

  private initializeFetchingTableItemsSubscription(): void {
    this.dataSource$ = this.fetchDate$.pipe(
      tap(() => this.areTableItemsLoadingSubject$.next(true)),
      switchMap(date => this.statisticsService.getSomeCustomEventsFromDate(this.slotKey, this.tablePageAmount, date, false).pipe(
        shareReplay(1),
        switchMap(events => this.statisticsService.getLastCustomStatistics(this.slotKey).pipe(
          map(lastStatistics => {
            this.dataSourceSubject$.next(this.getNewItems(lastStatistics, events));
            this.areTableItemsLoadingSubject$.next(false)
            return this.dataSourceSubject$.value;
          })
        )
        )),
      ));
  }

  openDetailMenu(serializedEvent: string) {
    this.dialogManager.openCustomEventDetails(serializedEvent);
  }
  trackById(index: number, item: CustomEventResponse): string {
    return item.id;
  }
  private getNewItems(lastStatistics: { key: string; statistics: CustomEventStatisticsResponse; } | null, events: CustomEventResponse[]) {
    const currentItems = this.dataSourceSubject$.value;
    let uniqueItems = getUniqueItems(currentItems, events) as CustomEventResponse[];
    let newItems = [...currentItems, ...uniqueItems];
    if (this.validateMessage(lastStatistics)) {
      const newEvent = lastStatistics?.statistics.lastEvent;
      uniqueItems = getUniqueItems(newItems, [newEvent!]) as CustomEventResponse[];
      newItems = [...uniqueItems, ...newItems];
    }
    return newItems;
  }
  private generateChartSeries(statistics: ServerLoadStatisticsResponse | undefined): Map<string, number> {
    let data = new Map<string, number>();
    if (statistics && statistics.loadMethodStatistics) {
      let methodStatistics = statistics.loadMethodStatistics;
      data.set("GET", methodStatistics.getAmount);
      data.set("POST", methodStatistics.postAmount);
      data.set("PUT", methodStatistics.putAmount);
      data.set("PATCH", methodStatistics.patchAmount);
      data.set("DELETE", methodStatistics.deleteAmount);
    }
    return data;
  }
  private validateMessage(message: { key: string; statistics: CustomEventStatisticsResponse; } | null) {
    return message
      && !message.statistics.isInitial
      && message.key === this.slotKey
      && message?.statistics?.lastEvent
  }
}