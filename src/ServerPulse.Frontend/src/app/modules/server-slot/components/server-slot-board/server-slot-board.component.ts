import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Store } from '@ngrx/store';
import { BehaviorSubject, debounceTime, map, Observable, of, startWith, switchMap } from 'rxjs';
import { environment } from '../../../../../environment/environment';
import { createServerSlot, CreateServerSlotRequest, getUserServerSlots, selectServerSlots, ServerSlot } from '../../../server-slot-shared';

@Component({
  selector: 'app-server-slot-board',
  templateUrl: './server-slot-board.component.html',
  styleUrl: './server-slot-board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ServerSlotBoardComponent implements OnInit {
  inputControl: FormControl = new FormControl('');

  private readonly slotAmountSubject$ = new BehaviorSubject<number>(0);
  serverSlots$: Observable<ServerSlot[]> = of([]);
  slotAmount$: Observable<number> = this.slotAmountSubject$.asObservable();

  get maxAmountOfSlots(): number {
    return environment.maxAmountOfSlotsPerUser;
  }

  constructor(
    private readonly store: Store
  ) { }

  ngOnInit(): void {
    this.serverSlots$ = this.inputControl.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      switchMap(searchString => {
        this.store.dispatch(getUserServerSlots({ str: searchString }));
        return this.store.select(selectServerSlots);
      }),
      map(slots => {
        this.slotAmountSubject$.next(slots.length);
        return slots;
      }),
    );
  }

  addServerSlot(): void {
    if (this.slotAmountSubject$.value < this.maxAmountOfSlots) {
      const request: CreateServerSlotRequest = {
        name: 'New Slot'
      };
      this.store.dispatch(createServerSlot({ req: request }));
      this.inputControl.setValue('');
    }
  }
}
