import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Store } from '@ngrx/store';
import { loginUser, startRegisterUser } from '../..';
import { ValidationMessage } from '../../../shared';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let storeSpy: jasmine.SpyObj<Store>;

  beforeEach(waitForAsync(() => {
    storeSpy = jasmine.createSpyObj<Store>(['dispatch', 'select']);
    const validationMessageSpyObj = jasmine.createSpyObj('ValidationMessage', ['getValidationMessage']);

    validationMessageSpyObj.getValidationMessage.and.returnValue({ hasError: false, message: "" });

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
        { provide: Store, useValue: storeSpy },
        { provide: ValidationMessage, useValue: validationMessageSpyObj }
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;

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
    expect(component.loginInput.hasError('required')).toBeTrue();
    expect(component.passwordInput.hasError('minlength')).toBeTrue();
  });

  it('should call signInUser on valid form submission', () => {
    const values = {
      login: 'john@example.com',
      password: 'Password123;'
    };
    component.formGroup.setValue(values);

    fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture.detectChanges();

    expect(storeSpy.dispatch).toHaveBeenCalledWith(
      loginUser({ req: values })
    );
  });

  it('should toggle password visibility on button click', () => {
    const passwordInput = fixture.debugElement.query(By.css('input[formControlName="password"]')).nativeElement;
    const visibilityToggle = fixture.debugElement.query(By.css('span[matSuffix]')).nativeElement;

    expect(passwordInput.type).toBe('password');

    visibilityToggle.click();
    fixture.detectChanges();

    expect(passwordInput.type).toBe('text');

    visibilityToggle.click();
    fixture.detectChanges();

    expect(passwordInput.type).toBe('password');
  });

  it('should toggle password visibility on Enter key press', () => {
    const event = new KeyboardEvent('keydown', { key: 'Enter' });
    component.hidePassword = true;

    component.hidePasswordOnKeydown(event);
    fixture.detectChanges();

    expect(component.hidePassword).toBeFalse();

    component.hidePasswordOnKeydown(event);
    fixture.detectChanges();

    expect(component.hidePassword).toBeTrue();
  });

  it('should dispatch StartRegistrationCommand on registration link click', () => {
    fixture.debugElement.query(By.css('button#to-register-link')).nativeElement.click();

    expect(storeSpy.dispatch).toHaveBeenCalledWith(
      startRegisterUser()
    );
  });
});
