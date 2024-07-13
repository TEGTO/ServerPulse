import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterModule, Routes } from '@angular/router';
import { ServerSlotDialogManager, ServerSlotDialogManagerService } from '.';
import { ServerSlotEditComponent } from './components/server-slot-edit/server-slot-edit.component';
import { ServerSlotInfoComponent } from './components/server-slot-info/server-slot-info.component';
import { ServerSlotComponent } from './components/server-slot/server-slot.component';
import { SlotBoardComponent } from './components/slot-board/slot-board.component';

const routes: Routes = [
  {
    path: "serverslot/:slotId", component: ServerSlotInfoComponent,
  }
];

@NgModule({
  declarations: [
    SlotBoardComponent,
    ServerSlotComponent,
    ServerSlotEditComponent,
    ServerSlotInfoComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forRoot(routes),
    MatButtonModule,
    MatDialogModule,
    MatInputModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
  ],
  providers: [
    { provide: ServerSlotDialogManager, useClass: ServerSlotDialogManagerService },
  ],
  exports: [
    SlotBoardComponent
  ],
})
export class ServerSlotsModule { }
