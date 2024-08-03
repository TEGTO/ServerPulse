import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatToolbarModule } from '@angular/material/toolbar';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule, Routes } from '@angular/router';
import { AuthInterceptor } from '../authentication';
import { AuthenticationModule } from '../authentication/authentication.module';
import { SlotBoardComponent } from '../server-slots';
import { ServerSlotsModule } from '../server-slots/server-slots.module';
import { CustomErrorHandler, ErrorHandler } from '../shared';
import { AppComponent, MainViewComponent } from './index';

const routes: Routes = [
  {
    path: "", component: MainViewComponent,
    children: [
      { path: "", component: SlotBoardComponent },
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
    HttpClientModule,
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: ErrorHandler, useClass: CustomErrorHandler },
  ],
  bootstrap: [AppComponent]
})
export class CoreModule { }
