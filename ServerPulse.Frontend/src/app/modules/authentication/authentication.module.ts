import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { provideEffects } from '@ngrx/effects';
import { provideState, provideStore } from '@ngrx/store';
import { AuthInterceptor, AuthenticatedComponent, AuthenticationControllerService, AuthenticationDialogManager, AuthenticationDialogManagerService, AuthenticationService, LoginComponent, RegisterComponent, RegistrationEffects, SignInEffects, authReducer, registrationReducer } from '.';

@NgModule({
  declarations: [LoginComponent, RegisterComponent, AuthenticatedComponent],
  imports: [
    CommonModule,
    MatDialogModule,
    MatInputModule,
    FormsModule,
    MatFormFieldModule,
    ReactiveFormsModule,
    MatButtonModule,
    HttpClientModule,
  ],
  providers: [
    { provide: AuthenticationDialogManager, useClass: AuthenticationDialogManagerService },
    { provide: AuthenticationService, useClass: AuthenticationControllerService },
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    provideStore(),
    provideState({ name: "registration", reducer: registrationReducer }),
    provideState({ name: "authentication", reducer: authReducer }),
    provideEffects(RegistrationEffects),
    provideEffects(SignInEffects),
  ],
  exports: [LoginComponent],
})
export class AuthenticationModule { }
