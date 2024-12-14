import { CommonModule } from '@angular/common';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatToolbarModule } from '@angular/material/toolbar';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule, Routes } from '@angular/router';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { AuthenticationModule } from '../authentication/authentication.module';
import { ServerSlotInfoComponent, SlotBoardComponent } from '../server-slots';
import { ServerSlotsModule } from '../server-slots/server-slots.module';
import { CustomErrorHandler, ErrorHandler, JsonDownloader, JsonDownloaderService, ValidationMessage, ValidationMessageService } from '../shared';
import { AppComponent, MainViewComponent } from './index';

const routes: Routes = [
  {
    path: "", component: MainViewComponent,
    children: [
      { path: "", component: SlotBoardComponent },
      { path: "serverslot/:id", component: ServerSlotInfoComponent },
    ]
  }
];
@NgModule({
  declarations: [
    AppComponent,
    MainViewComponent
  ],
  imports: [
    BrowserModule,
    CommonModule,
    RouterModule.forRoot(routes),
    BrowserAnimationsModule,
    MatToolbarModule,
    MatButtonModule,
    MatDialogModule,
    AuthenticationModule,
    MatDialogModule,
    ServerSlotsModule,
    StoreModule.forRoot({}, {}),
    EffectsModule.forRoot([]),
  ],
  providers: [
    provideHttpClient(
      withInterceptorsFromDi(),
    ),
    { provide: ErrorHandler, useClass: CustomErrorHandler },
    { provide: ValidationMessage, useClass: ValidationMessageService },
    { provide: JsonDownloader, useClass: JsonDownloaderService },
  ],
  bootstrap: [AppComponent]
})
export class CoreModule { }
