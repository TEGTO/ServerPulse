import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { catchError, of, Subject, switchMap, takeUntil, tap } from 'rxjs';
import { AuthenticationService } from '../..';
import { SnackbarManager, UserUpdateDataRequest } from '../../../shared';

@Component({
  selector: 'app-authenticated',
  templateUrl: './authenticated.component.html',
  styleUrl: './authenticated.component.scss'
})
export class AuthenticatedComponent implements OnInit, OnDestroy {
  hideNewPassword: boolean = true;
  userEmail: string = "";
  formGroup: FormGroup = null!;
  private destroy$ = new Subject<void>();

  get nameInput() { return this.formGroup.get('userName')!; }
  get emailInput() { return this.formGroup.get('email')!; }
  get oldPassword() { return this.formGroup.get('oldPassword')!; }
  get newPassword() { return this.formGroup.get('newPassword')!; }

  constructor(
    private readonly authService: AuthenticationService,
    private readonly snackbarManager: SnackbarManager
  ) { }

  ngOnInit(): void {
    this.authService.getUserData()
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => {
        this.userEmail = data.email;
        this.formGroup = new FormGroup(
          {
            userName: new FormControl(data.userName, [Validators.required, Validators.maxLength(256)]),
            email: new FormControl(data.email, [Validators.email, Validators.required, Validators.maxLength(256)]),
            oldPassword: new FormControl('', [Validators.required, Validators.maxLength(256)]),
            newPassword: new FormControl('', [Validators.minLength(8), Validators.maxLength(256)])
          });
      })
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  logOutUser() {
    this.authService.logOutUser();
  }
  updateUser() {
    if (this.formGroup.valid) {
      const userData: UserUpdateDataRequest = {
        userName: this.formGroup.value.userName,
        oldEmail: this.userEmail,
        newEmail: this.formGroup.value.email,
        oldPassword: this.formGroup.value.oldPassword,
        newPassword: this.formGroup.value.newPassword,
      };

      this.authService.updateUser(userData).pipe(
        takeUntil(this.destroy$),
        tap(isSuccess => {
          if (isSuccess) {
            this.snackbarManager.openInfoSnackbar("✔️ The update is successful!", 5);
          }
        }),
        switchMap(() => this.authService.getUserErrors()),
        tap(errors => {
          if (errors) {
            this.snackbarManager.openErrorSnackbar(errors.split("\n"));
          }
        }),
        catchError(err => {
          this.snackbarManager.openErrorSnackbar(["An error occurred while updating."]);
          return of(null);
        })
      ).subscribe();
    }
  }
}