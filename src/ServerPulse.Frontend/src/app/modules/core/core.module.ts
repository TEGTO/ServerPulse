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
import { AnalyzerModule } from '../analyzer/analyzer.module';
import { EmailCallbackComponent, getEmailConfirmRedirectPath, getOAuthRedirectPath, OAuthCallbackComponent } from '../authentication';
import { AuthenticationModule } from '../authentication/authentication.module';
import { CustomErrorHandler, ErrorHandler, JsonDownloader, JsonDownloaderService, ValidationMessage, ValidationMessageService } from '../shared';
import { AppComponent, MainViewComponent } from './index';

const routes: Routes = [
  {
    path: "", component: MainViewComponent,
    children: [
      {
        path: "",
        loadChildren: () => import('../server-slot/server-slot.module').then(m => m.ServerSlotModule),
      },
      {
        path: "info",
        loadChildren: () => import('../server-slot-info/server-slot-info.module').then(m => m.ServerSlotInfoModule),
      },
    ],
  },
  { path: getEmailConfirmRedirectPath(), component: EmailCallbackComponent },
  { path: getOAuthRedirectPath(), component: OAuthCallbackComponent },
  { path: '**', redirectTo: '' }
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
    AnalyzerModule,
    AuthenticationModule,
    MatDialogModule,
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
