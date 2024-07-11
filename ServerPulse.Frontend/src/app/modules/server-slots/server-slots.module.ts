import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ServerSlotComponent } from './components/server-slot/server-slot.component';
import { SlotBoardComponent } from './components/slot-board/slot-board.component';

@NgModule({
  declarations: [
    SlotBoardComponent,
    ServerSlotComponent
  ],
  imports: [
    CommonModule
  ],
  exports: [
    SlotBoardComponent
  ]
})
export class ServerSlotsModule { }
