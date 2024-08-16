import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { BehaviorSubject, debounceTime, map, Observable, of, startWith, switchMap } from 'rxjs';
import { environment } from '../../../../../environment/environment';
import { CreateServerSlotRequest, ServerSlot } from '../../../shared';
import { ServerSlotService } from '../../index';

@Component({
  selector: 'app-slot-board',
  templateUrl: './slot-board.component.html',
  styleUrls: ['./slot-board.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SlotBoardComponent implements OnInit {

  private slotAmountSubject$ = new BehaviorSubject<number>(0);

  serverSlots$: Observable<ServerSlot[]> = of([]);
  slotAmount$: Observable<number> = this.slotAmountSubject$.asObservable();
  inputControl: FormControl = new FormControl('');

  get maxAmountOfSlots(): number {
    return environment.maxAmountOfSlotsPerUser;
  }

  constructor(
    private readonly serverSlotService: ServerSlotService,
  ) { }

  ngOnInit(): void {
    this.serverSlots$ = this.inputControl.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      switchMap(searchString => {
        if (searchString) {
          return this.serverSlotService.getServerSlotsWithString(searchString);
        }
        else {
          return this.serverSlotService.getServerSlots();
        }
      }),
      map(slots => {
        this.slotAmountSubject$.next(slots.length);
        return slots;
      }),
    );
  }

  addServerSlot(): void {
    if (this.slotAmountSubject$.value < this.maxAmountOfSlots) {
      const newServerSlot: CreateServerSlotRequest = {
        name: 'New Slot'
      };
      this.serverSlotService.createServerSlot(newServerSlot);
      this.inputControl.setValue('');
    }
  }
}