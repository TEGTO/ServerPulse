import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { noSpaces, notEmptyString, ValidationMessage } from '../../../shared';
import { confirmPasswordValidator, passwordValidator, registerUser, startLoginUser, UserRegistrationRequest } from '../../index';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegisterComponent implements OnInit {
  formGroup!: FormGroup;
  hidePassword = true;

  get emailInput() { return this.formGroup.get('email') as FormControl; }
  get passwordInput() { return this.formGroup.get('password') as FormControl; }
  get passwordConfirmInput() { return this.formGroup.get('confirmPassword') as FormControl; }

  constructor(
    private readonly store: Store,
    private readonly validateInput: ValidationMessage,
  ) { }

  ngOnInit(): void {
    this.formGroup = new FormGroup(
      {
        email: new FormControl('', [Validators.required, notEmptyString, noSpaces, Validators.email, Validators.maxLength(256)]),
        password: new FormControl('', [Validators.required, notEmptyString, noSpaces, passwordValidator, Validators.maxLength(256)]),
        confirmPassword: new FormControl('', [Validators.required, notEmptyString, noSpaces, confirmPasswordValidator, Validators.maxLength(256)])
      });
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

  openLoginMenu() {
    this.store.dispatch(startLoginUser());
  }

  registerUser() {
    if (this.formGroup.valid) {
      const req: UserRegistrationRequest =
      {
        email: this.emailInput.value,
        password: this.passwordInput.value,
        confirmPassword: this.passwordConfirmInput.value,
      }
      this.store.dispatch(registerUser({ req: req }));
    }
    else {
      this.formGroup.markAllAsTouched();
    }
  }
}