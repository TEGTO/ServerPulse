import { Component } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { SnackbarManager, UserAuthenticationRequest } from '../../../shared';
import { AuthenticationDialogManager, AuthenticationService } from '../../index';

@Component({
  selector: 'auth-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  formGroup: FormGroup = new FormGroup(
    {
      login: new FormControl('', [Validators.required, Validators.maxLength(256)]),
      password: new FormControl('', [Validators.required, Validators.minLength(8), Validators.maxLength(256)]),
    });
  hidePassword: boolean = true;

  get loginInput() { return this.formGroup.get('login')!; }
  get passwordInput() { return this.formGroup.get('password')!; }

  constructor(
    private readonly authDialogManager: AuthenticationDialogManager,
    private readonly authService: AuthenticationService,
    private readonly dialogRef: MatDialogRef<LoginComponent>,
    private readonly snackbarManager: SnackbarManager
  ) { }

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
      this.authService.getAuthData().subscribe(authData => {
        if (authData.isAuthenticated) {
          this.dialogRef.close();
        }
        this.authService.getAuthErrors().subscribe(
          errors => {
            if (errors) {
              this.snackbarManager.openErrorSnackbar(errors.split("\n"));
            }
          });
      });
    }
  }
}