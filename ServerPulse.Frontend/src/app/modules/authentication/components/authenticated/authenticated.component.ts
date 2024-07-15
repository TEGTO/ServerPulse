import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { AuthenticationService } from '../..';
import { SnackbarManager, UserUpdateDataRequest } from '../../../shared';

@Component({
  selector: 'app-authenticated',
  templateUrl: './authenticated.component.html',
  styleUrl: './authenticated.component.scss'
})
export class AuthenticatedComponent implements OnInit {
  hideNewPassword: boolean = true;
  userEmail: string = "";
  formGroup: FormGroup = null!;

  get nameInput() { return this.formGroup.get('userName')!; }
  get emailInput() { return this.formGroup.get('email')!; }
  get oldPassword() { return this.formGroup.get('oldPassword')!; }
  get newPassword() { return this.formGroup.get('newPassword')!; }

  constructor(
    private readonly authService: AuthenticationService,
    private readonly snackbarManager: SnackbarManager
  ) { }

  ngOnInit(): void {
    this.authService.getUserData().subscribe(data => {
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
  logOutUser() {
    this.authService.logOutUser();
  }
  updateUser() {
    if (this.formGroup.valid) {
      const formValues = { ...this.formGroup.value };
      const userData: UserUpdateDataRequest = {
        userName: formValues.userName,
        oldEmail: this.userEmail,
        newEmail: formValues.email,
        oldPassword: formValues.oldPassword,
        newPassword: formValues.newPassword,
      };
      this.authService.updateUser(userData).subscribe(isSuccess => {
        if (isSuccess) {
          this.snackbarManager.openInfoSnackbar("✔️ The update is successful!", 5)
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