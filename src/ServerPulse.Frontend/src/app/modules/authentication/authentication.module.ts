import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { AuthEffects, AuthenticatedComponent, AuthenticationDialogManager, AuthenticationDialogManagerService, AuthInterceptor, authReducer, LoginComponent, RegisterComponent, UnauthenticatedComponent } from '.';

@NgModule({
  declarations: [
    LoginComponent,
    AuthenticatedComponent,
    RegisterComponent,
    UnauthenticatedComponent
  ],
  imports: [
    CommonModule,
    MatDialogModule,
    MatInputModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatButtonModule,
    StoreModule.forFeature('authentication', authReducer),
    EffectsModule.forFeature([AuthEffects]),
  ],
  providers: [
    provideHttpClient(
      withInterceptorsFromDi(),
    ),
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: AuthenticationDialogManager, useClass: AuthenticationDialogManagerService },
  ],
  exports: [UnauthenticatedComponent]
})
export class AuthenticationModule { }
