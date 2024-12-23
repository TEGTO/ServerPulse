import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { BehaviorSubject, filter, Observable, Subject, takeUntil, tap } from 'rxjs';
import { startLifecycleStatisticsReceiving, startLoadStatisticsReceiving } from '../../../analyzer';
import { deleteServerSlot, getServerSlotById, selectServerSlotById, ServerSlot, showSlotKey } from '../../../server-slot-shared';

@Component({
  selector: 'app-server-slot-info',
  templateUrl: './server-slot-info.component.html',
  styleUrl: './server-slot-info.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerSlotInfoComponent implements OnInit, OnDestroy {

  inputIsEditable$ = new BehaviorSubject<boolean>(false);
  slotId$ = new BehaviorSubject<string | null>(null);
  serverSlot$!: Observable<ServerSlot>;
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly store: Store,
    private readonly route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const slotId = params.get('id');
        this.slotId$.next(slotId);

        if (slotId) {

          this.store.dispatch(getServerSlotById({ id: slotId }));

          this.serverSlot$ = this.store.select(selectServerSlotById(slotId)).pipe(
            filter((x): x is ServerSlot => x !== undefined && x !== null),
            tap(serverSlot => {
              this.store.dispatch(startLoadStatisticsReceiving({ key: serverSlot.slotKey }));
              this.store.dispatch(startLifecycleStatisticsReceiving({ key: serverSlot.slotKey }));
            })
          );
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  showKey(serverSlot: ServerSlot) {
    this.store.dispatch(showSlotKey({ slot: serverSlot }));
  }

  makeInputEditable(): void {
    this.inputIsEditable$.next(true);
  }

  openConfirmDeletion(serverSlot: ServerSlot) {
    this.store.dispatch(deleteServerSlot({ id: serverSlot.id }));
  }
}
