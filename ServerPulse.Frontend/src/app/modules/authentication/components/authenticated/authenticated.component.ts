import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { AuthenticationService } from '../..';
import { UserUpdateDataRequest } from '../../../shared';

@Component({
  selector: 'app-authenticated',
  templateUrl: './authenticated.component.html',
  styleUrl: './authenticated.component.scss'
})
export class AuthenticatedComponent implements OnInit {
  hideNewPassword: boolean = true;
  userEmail: string = "";
  formGroup: FormGroup = null!;
  updateErrors: string[] = [];
  isUpdateSuccessful: boolean = false;

  get emailInput() { return this.formGroup.get('email')!; }
  get oldPassword() { return this.formGroup.get('oldPassword')!; }
  get newPassword() { return this.formGroup.get('newPassword')!; }

  constructor(private authService: AuthenticationService, private dialog: MatDialog) {
  }

  ngOnInit(): void {
    this.authService.getAuthUserData().subscribe(data => {
      this.userEmail = data.userEmail;
      this.formGroup = new FormGroup(
        {
          email: new FormControl(this.userEmail, [Validators.email, Validators.required]),
          oldPassword: new FormControl('', [Validators.required]),
          newPassword: new FormControl('', [Validators.minLength(8)])
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
        oldEmail: this.userEmail,
        newEmail: formValues.email,
        oldPassword: formValues.oldPassword,
        newPassword: formValues.newPassword,
      };
      this.authService.updateUser(userData).subscribe(isSuccess => {
        this.isUpdateSuccessful = isSuccess;
        this.authService.getAuthUserErrors().subscribe(
          errors => {
            if (errors)
              this.updateErrors = errors.split("\n");
          });
      });
    }
  }
}