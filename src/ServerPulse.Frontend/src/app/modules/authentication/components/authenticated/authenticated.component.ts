import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Subject, takeUntil } from 'rxjs';
import { changePasswordValidator, logOutUser, selectAuthData, updateUserData, UserUpdateRequest } from '../..';
import { noSpaces, notEmptyString, ValidationMessage } from '../../../shared';

@Component({
  selector: 'app-authenticated',
  templateUrl: './authenticated.component.html',
  styleUrl: './authenticated.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AuthenticatedComponent implements OnInit, OnDestroy {
  hideNewPassword = true;
  formGroup: FormGroup = null!;
  private readonly destroy$ = new Subject<void>();

  get emailInput() { return this.formGroup.get('email') as FormControl; }
  get oldPasswordInput() { return this.formGroup.get('oldPassword') as FormControl; }
  get passwordInput() { return this.formGroup.get('password') as FormControl; }

  constructor(
    private readonly store: Store,
    private readonly validateInput: ValidationMessage
  ) { }

  ngOnInit(): void {
    this.store.select(selectAuthData)
      .pipe(takeUntil(this.destroy$))
      .subscribe(data => {
        this.formGroup = new FormGroup(
          {
            email: new FormControl(data.email, [Validators.required, notEmptyString, noSpaces, Validators.email, Validators.maxLength(256)]),
            oldPassword: new FormControl('', [Validators.required, notEmptyString, noSpaces, Validators.maxLength(256)]),
            password: new FormControl('', [noSpaces, changePasswordValidator, Validators.maxLength(256)])
          });
      })
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  validateInputField(input: AbstractControl<any, any>) {
    return this.validateInput.getValidationMessage(input);
  }

  hidePasswordOnKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.hideNewPassword = !this.hideNewPassword;
    }
  }

  updateUser(): void {
    if (this.formGroup.valid) {
      const req: UserUpdateRequest = {
        email: this.emailInput.value,
        oldPassword: this.oldPasswordInput.value,
        password: this.passwordInput.value,
      };
      this.store.dispatch(updateUserData({ req: req }));
    }
    else {
      this.formGroup.markAllAsTouched();
    }
  }

  logOutUser(): void {
    this.store.dispatch(logOutUser());
  }
}
