import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { debounceTime, Subject } from 'rxjs';
import { ServerSlotService } from '../..';
import { environment } from '../../../../../environment/environment';
import { CreateServerSlotRequest, ServerSlot } from '../../../shared';

@Component({
  selector: 'app-slot-board',
  templateUrl: './slot-board.component.html',
  styleUrl: './slot-board.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SlotBoardComponent implements OnInit {
  serverSlots: ServerSlot[] = [];
  private inputSubject: Subject<string> = new Subject<string>();

  get slotAmount() { return this.serverSlots.length; }
  get maxAmountOfSlots() { return environment.maxAmountOfSlotsPerUser; }

  constructor(
    private readonly serverSlotService: ServerSlotService,
    private readonly cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.serverSlotService.getServerSlots().subscribe(slots => {
      this.serverSlots = slots;
      this.cdr.markForCheck();
    });
    this.inputSubject.pipe(
      debounceTime(500)
    ).subscribe(str => {
      this.serverSlotService.getServerSlotsWithString(str).subscribe(slots => {
        this.serverSlots = slots;
        this.cdr.markForCheck();
      });
    });
  }

  addServerSlot() {
    if (this.slotAmount < this.maxAmountOfSlots) {
      let newServerSlot: CreateServerSlotRequest = {
        name: "New Slot"
      };
      this.serverSlotService.createServerSlot(newServerSlot);
    }
  }

  onInputChange(event: Event) {
    const inputElement = event.target as HTMLInputElement;
    let str = inputElement.value;
    this.inputSubject.next(str);
  }
}