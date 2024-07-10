import { Component } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { AuthenticationService, confirmPasswordValidator } from '../..';
import { UserRegistrationRequest } from '../../../shared';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  formGroup: FormGroup = new FormGroup(
    {
      userName: new FormControl('', [Validators.required, Validators.maxLength(256)]),
      email: new FormControl('', [Validators.email, Validators.required, Validators.maxLength(256)]),
      password: new FormControl('', [Validators.required, Validators.minLength(8), Validators.maxLength(256)]),
      passwordConfirm: new FormControl('', [Validators.required, confirmPasswordValidator, Validators.maxLength(256)])
    });
  hidePassword: boolean = true;
  registerErrors: string[] = [];

  get emailInput() { return this.formGroup.get('email')!; }
  get passwordInput() { return this.formGroup.get('password')!; }
  get passwordConfirmInput() { return this.formGroup.get('passwordConfirm')!; }

  constructor(private authService: AuthenticationService, private dialogRef: MatDialogRef<RegisterComponent>) { }

  registerUser() {
    if (this.formGroup.valid) {
      const formValues = { ...this.formGroup.value };
      const userData: UserRegistrationRequest = {
        userName: formValues.userName,
        email: formValues.email,
        password: formValues.password,
        confirmPassword: formValues.passwordConfirm
      };
      this.authService.registerUser(userData).subscribe(isSuccess => {
        if (isSuccess) {
          this.dialogRef.close();
        }
        else {
          this.authService.registerUserGetErrors().subscribe(errors => {
            if (errors)
              this.registerErrors = errors.split("\n");
          })
        }
      });
    }
  }
}
