import { ScrollingModule } from '@angular/cdk/scrolling';
import { CdkTableModule } from '@angular/cdk/table';
import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { RouterModule, Routes } from '@angular/router';
import { ServerSlotActivityPreviewComponent, ServerSlotBoardComponent, ServerSlotComponent } from '.';
import { ChartModule } from '../chart/chart.module';
import { ServerSlotSharedModule } from '../server-slot-shared/server-slot-shared.module';

const routes: Routes = [
  {
    path: "",
    component: ServerSlotBoardComponent,
  },
];

@NgModule({
  declarations: [
    ServerSlotComponent,
    ServerSlotBoardComponent,
    ServerSlotActivityPreviewComponent,
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    ServerSlotSharedModule,
    ChartModule,
    MatButtonModule,
    MatDialogModule,
    CdkTableModule,
    MatInputModule,
    ScrollingModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatSelectModule,
    MatMenuModule,
    MatProgressSpinnerModule,
  ]
})
export class ServerSlotModule { }
