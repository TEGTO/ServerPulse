import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { AuthenticationService } from '../..';
import { SnackbarManager } from '../../../shared';
import { AuthenticatedComponent } from './authenticated.component';

describe('AuthenticatedComponent', () => {
  let component: AuthenticatedComponent;
  let fixture: ComponentFixture<AuthenticatedComponent>;
  let authService: jasmine.SpyObj<AuthenticationService>;
  let snackbarManager: jasmine.SpyObj<SnackbarManager>;

  beforeEach(waitForAsync(() => {
    const authServiceSpy = jasmine.createSpyObj('AuthenticationService', ['getUserData', 'updateUser', 'logOutUser', 'getUserErrors']);
    const snackbarManagerSpy = jasmine.createSpyObj('SnackbarManager', ['openInfoSnackbar', 'openErrorSnackbar']);

    TestBed.configureTestingModule({
      declarations: [AuthenticatedComponent],
      imports: [
        ReactiveFormsModule,
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        NoopAnimationsModule,
      ],
      providers: [
        { provide: AuthenticationService, useValue: authServiceSpy },
        { provide: SnackbarManager, useValue: snackbarManagerSpy },
        { provide: MatDialogRef, useValue: {} },
        { provide: MAT_DIALOG_DATA, useValue: {} }
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(AuthenticatedComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthenticationService) as jasmine.SpyObj<AuthenticationService>;
    snackbarManager = TestBed.inject(SnackbarManager) as jasmine.SpyObj<SnackbarManager>;

    authService.getUserData.and.returnValue(of({ userName: 'John Doe', email: 'john@example.com' }));
  }));

  beforeEach(() => {
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display validation errors', () => {
    const inputs = fixture.debugElement.queryAll(By.css('input'));

    const nameInput = inputs[0].nativeElement;
    nameInput.value = '';
    nameInput.dispatchEvent(new Event('input'));
    nameInput.blur();
    fixture.detectChanges();

    const emailInput = inputs[1].nativeElement;
    emailInput.value = 'invalid email';
    emailInput.dispatchEvent(new Event('input'));
    emailInput.blur();
    fixture.detectChanges();

    const oldPasswordInput = inputs[2].nativeElement;
    oldPasswordInput.value = '';
    oldPasswordInput.dispatchEvent(new Event('input'));
    oldPasswordInput.blur();
    fixture.detectChanges();

    const newPasswordInput = inputs[3].nativeElement;
    newPasswordInput.value = 'short';
    newPasswordInput.dispatchEvent(new Event('input'));
    newPasswordInput.blur();
    fixture.detectChanges();

    expect(component.formGroup.valid).toBeFalse();
    expect(component.nameInput.hasError('required')).toBeTruthy();
    expect(component.emailInput.hasError('email')).toBeTruthy();
    expect(component.oldPassword.hasError('required')).toBeTruthy();
    expect(component.newPassword.hasError('minlength')).toBeTruthy();
  });

  it('should call updateUser on valid form submission', () => {
    component.formGroup.setValue({
      userName: 'John Doe',
      email: 'john@example.com',
      oldPassword: 'oldpassword',
      newPassword: 'newpassword123'
    });

    authService.updateUser.and.returnValue(of(true));
    authService.getUserErrors.and.returnValue(of(null));

    fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture.detectChanges();

    expect(authService.updateUser).toHaveBeenCalledWith({
      userName: 'John Doe',
      oldEmail: 'john@example.com',
      newEmail: 'john@example.com',
      oldPassword: 'oldpassword',
      newPassword: 'newpassword123'
    });

    expect(snackbarManager.openInfoSnackbar).toHaveBeenCalledWith('✔️ The update is successful!', 5);
  });

  it('should display error messages on update failure', () => {
    component.formGroup.setValue({
      userName: 'John Doe',
      email: 'john@example.com',
      oldPassword: 'oldpassword',
      newPassword: 'newpassword123'
    });

    authService.updateUser.and.returnValue(of(false));
    authService.getUserErrors.and.returnValue(of('Update failed'));

    fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture.detectChanges();

    expect(authService.updateUser).toHaveBeenCalled();
    expect(snackbarManager.openErrorSnackbar).toHaveBeenCalledWith(['Update failed']);
  });

  it('should handle update errors', () => {
    const formValues = {
      userName: 'John Doe',
      email: 'john@example.com',
      oldPassword: 'oldpassword',
      newPassword: 'newpassword123'
    };

    component.formGroup.setValue(formValues);
    authService.updateUser.and.returnValue(of(false));
    authService.getUserErrors.and.returnValue(of('Server error'));

    component.updateUser();

    expect(authService.updateUser).toHaveBeenCalled();
    expect(snackbarManager.openErrorSnackbar).toHaveBeenCalledWith(['Server error']);
  });

  it('should call logOutUser on logout button click', () => {
    fixture.debugElement.query(By.css('button#logout-button')).nativeElement.click();
    expect(authService.logOutUser).toHaveBeenCalled();
  });
});
