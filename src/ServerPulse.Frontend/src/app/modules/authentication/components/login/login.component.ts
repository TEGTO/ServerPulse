import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Subject } from 'rxjs';
import { loginUser, OAuthLoginProvider, passwordValidator, startOAuthLogin, startRegisterUser, UserAuthenticationRequest } from '../..';
import { noSpaces, notEmptyString, ValidationMessage } from '../../../shared';

@Component({
  selector: 'app-auth-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginComponent implements OnInit, OnDestroy {
  formGroup!: FormGroup;
  hidePassword = true;
  private readonly destroy$ = new Subject<void>();

  get loginInput() { return this.formGroup.get('login') as FormControl; }
  get passwordInput() { return this.formGroup.get('password') as FormControl; }

  constructor(
    private readonly store: Store,
    private readonly validateInput: ValidationMessage
  ) { }

  ngOnInit(): void {
    this.formGroup = new FormGroup(
      {
        login: new FormControl('', [Validators.required, notEmptyString, noSpaces, Validators.email, Validators.maxLength(256)]),
        password: new FormControl('', [Validators.required, notEmptyString, noSpaces, passwordValidator, Validators.maxLength(256)]),
      });
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
      this.hidePassword = !this.hidePassword;
    }
  }

  openRegisterMenu(): void {
    this.store.dispatch(startRegisterUser());
  }

  loginUser(): void {
    if (this.formGroup.valid) {
      const req: UserAuthenticationRequest =
      {
        login: this.loginInput.value,
        password: this.passwordInput.value,
      }
      this.store.dispatch(loginUser({ req: req }));
    }
    else {
      this.formGroup.markAllAsTouched();
    }
  }

  googleOAuthLogin(): void {
    this.store.dispatch(startOAuthLogin({ loginProvider: OAuthLoginProvider.Google }));
  }
}