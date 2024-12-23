import { ChangeDetectionStrategy, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Store } from '@ngrx/store';
import { BehaviorSubject, Subject, takeUntil, tap } from 'rxjs';
import { selectLastLifecycleStatisticsByKey, ServerLifecycleStatistics, startLifecycleStatisticsReceiving } from '../../../analyzer';
import { deleteServerSlot, ServerSlot, ServerStatus, showSlotInfo, showSlotKey } from '../../../server-slot-shared';

@Component({
  selector: 'app-server-slot',
  templateUrl: './server-slot.component.html',
  styleUrl: './server-slot.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerSlotComponent implements OnInit, OnDestroy {
  @Input({ required: true }) serverSlot!: ServerSlot;

  inputIsEditable$ = new BehaviorSubject<boolean>(false);
  serverStatus$ = new BehaviorSubject<ServerStatus>(ServerStatus.NoData);
  private readonly destroy$ = new Subject<void>();

  constructor(private readonly store: Store) { }

  ngOnInit(): void {
    this.store.dispatch(startLifecycleStatisticsReceiving({ key: this.serverSlot.slotKey }));

    this.store.select(selectLastLifecycleStatisticsByKey(this.serverSlot.slotKey)).pipe(
      tap(statistics => {
        if (statistics) {
          this.updateServerStatus(statistics)
        }
      }),
      takeUntil(this.destroy$)
    ).subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateServerStatus(lifeCycleStatistics: ServerLifecycleStatistics): void {
    if (lifeCycleStatistics.dataExists) {
      this.serverStatus$.next(lifeCycleStatistics.isAlive ? ServerStatus.Online : ServerStatus.Offline);
    } else {
      this.serverStatus$.next(ServerStatus.NoData);
    }
  }

  showSlotInfo(): void {
    this.store.dispatch(showSlotInfo({ id: this.serverSlot.id }));
  }

  showKey(): void {
    this.store.dispatch(showSlotKey({ slot: this.serverSlot }));
  }

  makeInputEditable(): void {
    this.inputIsEditable$.next(true);
  }

  deleteSlot(): void {
    this.store.dispatch(deleteServerSlot({ id: this.serverSlot.id }));
  }
}
