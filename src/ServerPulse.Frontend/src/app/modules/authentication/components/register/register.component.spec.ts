import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Store } from '@ngrx/store';
import { getFullEmailConfirmRedirectPath, registerUser } from '../..';
import { ValidationMessage } from '../../../shared';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let storeSpy: jasmine.SpyObj<Store>;

  beforeEach(async () => {
    storeSpy = jasmine.createSpyObj<Store>(['dispatch', 'select']);
    const validationMessageSpy = jasmine.createSpyObj<ValidationMessage>(['getValidationMessage']);

    validationMessageSpy.getValidationMessage.and.returnValue({ hasError: false, message: "" });

    await TestBed.configureTestingModule({
      declarations: [RegisterComponent],
      imports: [
        ReactiveFormsModule,
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: Store, useValue: storeSpy },
        { provide: ValidationMessage, useValue: validationMessageSpy },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;

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

    const passwordInput = inputs[1].nativeElement;
    passwordInput.value = 'short';
    passwordInput.dispatchEvent(new Event('input'));
    passwordInput.blur();
    fixture.detectChanges();

    const confirmPasswordInput = inputs[2].nativeElement;
    confirmPasswordInput.value = 'mismatch';
    confirmPasswordInput.dispatchEvent(new Event('input'));
    confirmPasswordInput.blur();
    fixture.detectChanges();

    expect(component.formGroup.valid).toBeFalse();
    expect(component.emailInput.hasError('required')).toBeTruthy();
    expect(component.passwordInput.hasError('minlength')).toBeTruthy();
    expect(component.passwordConfirmInput.hasError('passwordNoMatch')).toBeTruthy();
  });

  it('should call registerUser on valid form submission', () => {
    const formValues = {
      redirectConfirmUrl: getFullEmailConfirmRedirectPath(),
      email: 'example@gmail,com',
      password: 'Password123',
      confirmPassword: 'Password123',
    };

    component.formGroup = new FormGroup({
      email: new FormControl(formValues.email, [Validators.required, Validators.maxLength(256)]),
      password: new FormControl(formValues.password, [Validators.required, Validators.minLength(8), Validators.maxLength(256)]),
      confirmPassword: new FormControl(formValues.confirmPassword, [Validators.required, Validators.maxLength(256)]),
    });

    fixture.detectChanges();

    fixture.debugElement.query(By.css('button[type="submit"]')).nativeElement.click();
    fixture.detectChanges();

    expect(storeSpy.dispatch).toHaveBeenCalledWith(registerUser({ req: formValues }));
  });
});
