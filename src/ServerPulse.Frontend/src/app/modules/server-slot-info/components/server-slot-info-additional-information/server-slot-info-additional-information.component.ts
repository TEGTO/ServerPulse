import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { AfterViewInit, Component, Input, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Store } from '@ngrx/store';
import { filter, map, Observable, of, pairwise, Subject, switchMap, takeUntil, tap, throttleTime, withLatestFrom } from 'rxjs';
import { addNewCustomEvent, getSomeCustomEvents, selectCustomEvents, selectCustomReadFromDate, selectSelectedDate, setCustomReadFromDate, showCustomDetailsEvent } from '../..';
import { CustomEvent, GetSomeMessagesRequest, selectLastCustomEventByKey, selectLastLoadStatisticsByKey, ServerLoadStatistics } from '../../../analyzer';
import { isSelectedDateToday } from '../../utils';

@Component({
  selector: 'app-server-slot-info-additional-information',
  templateUrl: './server-slot-info-additional-information.component.html',
  styleUrl: './server-slot-info-additional-information.component.scss'
})
export class ServerSlotInfoAdditionalInformationComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('scroller') scroller!: CdkVirtualScrollViewport;
  @Input({ required: true }) slotKey!: string;

  customEvents$!: Observable<CustomEvent[]>;
  chartData$: Observable<Map<string, number>> = of(new Map<string, number>);
  private readonly destroy$ = new Subject<void>();

  private previousDate: Date = new Date();
  readonly tableItemHeight = 65;
  private readonly tablePageAmount = 12;

  constructor(
    private readonly store: Store
  ) { }

  ngOnInit(): void {
    this.chartData$ = this.store.select(selectLastLoadStatisticsByKey(this.slotKey)).pipe(
      map(statistics => this.generateChartSeries(statistics)),
    );

    this.store.select(selectLastCustomEventByKey(this.slotKey))
      .pipe(
        takeUntil(this.destroy$),
        withLatestFrom(this.store.select(selectSelectedDate)),
        tap(([event, date]) => {
          if (event && isSelectedDateToday(date)) {
            this.store.dispatch(addNewCustomEvent({ event }))
          }
        })
      )
      .subscribe();

    this.customEvents$ = this.store.select(selectCustomReadFromDate).pipe(
      switchMap((date) => {
        const req: GetSomeMessagesRequest = {
          key: this.slotKey,
          numberOfMessages: this.tablePageAmount,
          startDate: date,
          readNew: false,
        };
        this.store.dispatch(getSomeCustomEvents({ req: req }));
        return this.store.select(selectCustomEvents);
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
        map(() => this.scroller.measureScrollOffset('bottom')),
        pairwise(),
        filter(([previous, current]) => current < previous && current < 2 * this.tableItemHeight),
        throttleTime(200),
        takeUntil(this.destroy$),
        withLatestFrom(this.store.select(selectCustomEvents)))
      .subscribe(([, events]) => {
        const length = events.length;
        const lastFetchedDate = new Date(length > 0 ? events[length - 1].creationDateUTC : this.previousDate);
        if (lastFetchedDate.getTime() != this.previousDate.getTime()) {
          this.previousDate = lastFetchedDate;
          this.store.dispatch(setCustomReadFromDate({ date: lastFetchedDate }));
        }
      });
  }

  openDetailMenu(event: CustomEvent): void {
    this.store.dispatch(showCustomDetailsEvent({ event }))
  }

  trackById(index: number, item: CustomEvent): string {
    return item.id;
  }

  private generateChartSeries(statistics: ServerLoadStatistics | null): Map<string, number> {
    const data = new Map<string, number>();
    if (statistics?.loadMethodStatistics) {
      const methodStatistics = statistics.loadMethodStatistics;
      data.set("GET", methodStatistics.getAmount);
      data.set("POST", methodStatistics.postAmount);
      data.set("PUT", methodStatistics.putAmount);
      data.set("PATCH", methodStatistics.patchAmount);
      data.set("DELETE", methodStatistics.deleteAmount);
    }
    return data;
  }
}
