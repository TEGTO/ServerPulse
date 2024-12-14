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

  get emailInput() { return this.formGroup.get('email')!; }
  get passwordInput() { return this.formGroup.get('password')!; }
  get passwordConfirmInput() { return this.formGroup.get('passwordConfirm')!; }

  constructor(
    private readonly store: Store,
    private readonly validateInput: ValidationMessage,
  ) { }

  ngOnInit(): void {
    this.formGroup = new FormGroup(
      {
        email: new FormControl('', [Validators.required, notEmptyString, noSpaces, Validators.email, Validators.maxLength(256)]),
        password: new FormControl('', [Validators.required, notEmptyString, noSpaces, passwordValidator, Validators.maxLength(256)]),
        passwordConfirm: new FormControl('', [Validators.required, notEmptyString, noSpaces, confirmPasswordValidator, Validators.maxLength(256)])
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
      const formValues = { ...this.formGroup.value };
      const req: UserRegistrationRequest =
      {
        email: formValues.email,
        password: formValues.password,
        confirmPassword: formValues.passwordConfirm,
      }
      this.store.dispatch(registerUser({ req: req }));
    }
    else {
      this.formGroup.markAllAsTouched();
    }
  }
}