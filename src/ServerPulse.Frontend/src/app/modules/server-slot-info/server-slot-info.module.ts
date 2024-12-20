import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ServerSlotInfoComponent } from '.';

const routes: Routes = [
  {
    path: ":id",
    component: ServerSlotInfoComponent,
  },
];

@NgModule({
  declarations: [
    ServerSlotInfoComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
  ]
})
export class ServerSlotInfoModule { }
