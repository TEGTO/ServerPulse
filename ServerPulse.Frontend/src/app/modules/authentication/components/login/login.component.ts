import { Component } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { SnackbarManager, UserAuthenticationRequest } from '../../../shared';
import { AuthenticationDialogManager, AuthenticationService, RegisterComponent } from '../../index';

@Component({
  selector: 'auth-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  formGroup: FormGroup = new FormGroup(
    {
      email: new FormControl('', [Validators.email, Validators.required, Validators.maxLength(256)]),
      password: new FormControl('', [Validators.required, Validators.minLength(8), Validators.maxLength(256)]),
    });
  hidePassword: boolean = true;

  get emailInput() { return this.formGroup.get('email')!; }
  get passwordInput() { return this.formGroup.get('password')!; }

  constructor(private authDialogManager: AuthenticationDialogManager,
    private authService: AuthenticationService,
    private dialogRef: MatDialogRef<RegisterComponent>,
    private snackbarManager: SnackbarManager) { }

  openRegisterMenu() {
    const dialogRef = this.authDialogManager.openRegisterMenu();
  }
  signInUser() {
    if (this.formGroup.valid) {
      const formValues = { ...this.formGroup.value };
      const userData: UserAuthenticationRequest = {
        email: formValues.email,
        password: formValues.password,
      };
      this.authService.singInUser(userData).subscribe(authData => {
        if (authData.isAuthenticated) {
          this.dialogRef.close();
        }
        this.authService.getAuthErrors().subscribe(
          errors => {
            if (errors)
              this.snackbarManager.openErrorSnackbar(errors.split("\n"));
          });
      });
    }
  }
}