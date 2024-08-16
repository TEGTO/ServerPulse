import { Component, OnDestroy } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { filter, Subject, switchMap, takeUntil, tap } from 'rxjs';
import { SnackbarManager, UserAuthenticationRequest } from '../../../shared';
import { AuthenticationDialogManager, AuthenticationService } from '../../index';

@Component({
  selector: 'auth-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnDestroy {
  formGroup: FormGroup = new FormGroup(
    {
      login: new FormControl('', [Validators.required, Validators.maxLength(256)]),
      password: new FormControl('', [Validators.required, Validators.minLength(8), Validators.maxLength(256)]),
    });
  hidePassword: boolean = true;
  private destroy$ = new Subject<void>();

  get loginInput() { return this.formGroup.get('login')!; }
  get passwordInput() { return this.formGroup.get('password')!; }

  constructor(
    private readonly authDialogManager: AuthenticationDialogManager,
    private readonly authService: AuthenticationService,
    private readonly dialogRef: MatDialogRef<LoginComponent>,
    private readonly snackbarManager: SnackbarManager
  ) { }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  openRegisterMenu() {
    const dialogRef = this.authDialogManager.openRegisterMenu();
  }
  signInUser() {
    if (this.formGroup.valid) {
      const formValues = { ...this.formGroup.value };
      const userData: UserAuthenticationRequest = {
        login: formValues.login,
        password: formValues.password,
      };
      this.authService.singInUser(userData);
      this.authService.getAuthData().pipe(
        takeUntil(this.destroy$),
        tap(authData => {
          if (authData.isAuthenticated) {
            this.dialogRef.close();
          }
        }),
        filter(authData => !authData.isAuthenticated),
        switchMap(() => this.authService.getAuthErrors()),
        takeUntil(this.destroy$),
        tap(errors => {
          if (errors) {
            this.snackbarManager.openErrorSnackbar(errors.split("\n"));
          }
        })
      ).subscribe();
    }
  }
}