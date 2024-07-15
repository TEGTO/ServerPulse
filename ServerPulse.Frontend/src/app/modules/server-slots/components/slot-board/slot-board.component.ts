import { Component, OnInit } from '@angular/core';
import { ServerSlotService } from '../..';
import { environment } from '../../../../../environment/environment';
import { CreateServerSlotRequest, ServerSlot } from '../../../shared';

@Component({
  selector: 'app-slot-board',
  templateUrl: './slot-board.component.html',
  styleUrl: './slot-board.component.scss'
})
export class SlotBoardComponent implements OnInit {

  serverSlots: ServerSlot[] = [];

  get slotAmount() { return this.serverSlots.length; }
  get maxAmountOfSlots() { return environment.maxAmountOfSlotsPerUser; }

  constructor(
    private readonly serverSlotService: ServerSlotService,
  ) { }

  ngOnInit(): void {
    this.serverSlotService.getServerSlots().subscribe(slots => {
      this.serverSlots = slots;
    });
  }
  addServerSlot() {
    if (this.slotAmount < this.maxAmountOfSlots) {
      let newServerSlot: CreateServerSlotRequest =
      {
        name: "New Slot"
      }
      this.serverSlotService.createServerSlot(newServerSlot);
    }
  }
  onInputChange(event: Event) {
    const inputElement = event.target as HTMLInputElement;
    let str = inputElement.value;
    this.serverSlotService.getServerSlotsWithString(str).subscribe(slots => {
      this.serverSlots = slots;
    });
  }
}
