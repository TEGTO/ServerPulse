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
import { AuthenticationDialogManager, AuthenticationService } from '../..';
import { AuthData, SnackbarManager } from '../../../shared';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: jasmine.SpyObj<AuthenticationService>;
  let snackbarManager: jasmine.SpyObj<SnackbarManager>;
  let authDialogManager: jasmine.SpyObj<AuthenticationDialogManager>;
  let dialogRef: jasmine.SpyObj<MatDialogRef<LoginComponent>>;

  beforeEach(waitForAsync(() => {
    const authServiceSpy = jasmine.createSpyObj('AuthenticationService', ['singInUser', 'getAuthData', 'getAuthErrors']);
    const snackbarManagerSpy = jasmine.createSpyObj('SnackbarManager', ['openErrorSnackbar']);
    const authDialogManagerSpy = jasmine.createSpyObj('AuthenticationDialogManager', ['openRegisterMenu']);
    const dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['close']);

    TestBed.configureTestingModule({
      declarations: [LoginComponent],
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
        { provide: AuthenticationDialogManager, useValue: authDialogManagerSpy },
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MAT_DIALOG_DATA, useValue: {} }
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthenticationService) as jasmine.SpyObj<AuthenticationService>;
    snackbarManager = TestBed.inject(SnackbarManager) as jasmine.SpyObj<SnackbarManager>;
    authDialogManager = TestBed.inject(AuthenticationDialogManager) as jasmine.SpyObj<AuthenticationDialogManager>;
    dialogRef = TestBed.inject(MatDialogRef) as jasmine.SpyObj<MatDialogRef<LoginComponent>>;

    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display validation errors', () => {
    const inputs = fixture.debugElement.queryAll(By.css('input'));

    const loginInput = inputs[0].nativeElement;
    loginInput.value = '';
    loginInput.dispatchEvent(new Event('input'));
    loginInput.blur();
    fixture.detectChanges();

    const passwordInput = inputs[1].nativeElement;
    passwordInput.value = 'short';
    passwordInput.dispatchEvent(new Event('input'));
    passwordInput.blur();
    fixture.detectChanges();

    expect(component.formGroup.valid).toBeFalse();
    expect(component.loginInput.hasError('required')).toBeTruthy();
    expect(component.passwordInput.hasError('minlength')).toBeTruthy();
  });

  it('should call signInUser on valid form submission', () => {
    const authData: AuthData =
    {
      isAuthenticated: true,
      accessToken: "accessToken",
      refreshToken: "refreshToken",
      refreshTokenExpiryDate: new Date()
    }
    component.formGroup.setValue({
      login: 'john@example.com',
      password: 'password123'
    });

    authService.getAuthData.and.returnValue(of(authData));
    authService.getAuthErrors.and.returnValue(of(null));

    fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture.detectChanges();

    expect(authService.singInUser).toHaveBeenCalledWith({
      login: 'john@example.com',
      password: 'password123'
    });
    expect(dialogRef.close).toHaveBeenCalled();
  });

  it('should display error messages on login failure', () => {
    const authData: AuthData =
    {
      isAuthenticated: true,
      accessToken: "accessToken",
      refreshToken: "refreshToken",
      refreshTokenExpiryDate: new Date()
    }
    component.formGroup.setValue({
      login: 'john@example.com',
      password: 'password123'
    });

    authService.getAuthData.and.returnValue(of(authData));
    authService.getAuthErrors.and.returnValue(of('Login failed'));

    fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture.detectChanges();

    expect(authService.singInUser).toHaveBeenCalled();
    expect(snackbarManager.openErrorSnackbar).toHaveBeenCalledWith(['Login failed']);
  });

  it('should handle login errors', () => {
    component.formGroup.setValue({
      login: 'john@example.com',
      password: 'password123'
    });
    const authData: AuthData =
    {
      isAuthenticated: false,
      accessToken: "accessToken",
      refreshToken: "refreshToken",
      refreshTokenExpiryDate: new Date()
    }
    authService.getAuthData.and.returnValue(of(authData));
    authService.getAuthErrors.and.returnValue(of('Server error'));

    component.signInUser();

    expect(authService.singInUser).toHaveBeenCalled();
    expect(snackbarManager.openErrorSnackbar).toHaveBeenCalledWith(['Server error']);
  });

  it('should open register menu on link click', () => {
    fixture.debugElement.query(By.css('a#to-register-link')).nativeElement.click();
    expect(authDialogManager.openRegisterMenu).toHaveBeenCalled();
  });
});