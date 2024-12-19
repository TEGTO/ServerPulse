import { ChangeDetectorRef, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { getDefaultAuthData, logOutUser, updateUserData, UserUpdateRequest } from '../..';
import { ValidationMessage } from '../../../shared';
import { AuthenticatedComponent } from './authenticated.component';

describe('AuthenticatedComponent', () => {
  let component: AuthenticatedComponent;
  let fixture: ComponentFixture<AuthenticatedComponent>;
  let storeSpy: jasmine.SpyObj<Store>;
  let mockValidationMessage: jasmine.SpyObj<ValidationMessage>;

  beforeEach(waitForAsync(() => {
    storeSpy = jasmine.createSpyObj<Store>(['dispatch', 'select']);
    mockValidationMessage = jasmine.createSpyObj<ValidationMessage>(['getValidationMessage']);

    mockValidationMessage.getValidationMessage.and.returnValue({ hasError: false, message: '' });

    storeSpy.select.and.callFake(() => {
      return of(
        {
          ...getDefaultAuthData(),
          email: 'test@example.com',
          isAuthenticated: true
        }
      )
    });

    TestBed.configureTestingModule({
      imports: [ReactiveFormsModule],
      declarations: [AuthenticatedComponent],
      providers: [
        { provide: Store, useValue: storeSpy },
        { provide: ValidationMessage, useValue: mockValidationMessage },
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    })
      .compileComponents();

    fixture = TestBed.createComponent(AuthenticatedComponent);
    component = fixture.componentInstance;
  }));

  beforeEach(() => {
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should load user data on initialization', () => {
    component.ngOnInit();
    fixture.detectChanges();

    expect(storeSpy.select).toHaveBeenCalled();
    expect(component.formGroup.get('email')?.value).toBe('test@example.com');
  });

  it('should call logOutUser on logout button click', () => {
    const logoutButton = fixture.debugElement.query(By.css('button#logout-button')).nativeElement;
    logoutButton.click();

    expect(storeSpy.dispatch).toHaveBeenCalledWith(logOutUser());
  });

  it('should call updateUser when form is valid and submit button is clicked', () => {

    const values: UserUpdateRequest = {
      email: 'updated@example.com',
      oldPassword: 'oldPassword123',
      password: 'newPassword123;',
    };

    component.formGroup.setValue(values);

    const saveButton = fixture.debugElement.query(By.css('button#send-button')).nativeElement;
    saveButton.click();

    expect(storeSpy.dispatch).toHaveBeenCalledWith(
      updateUserData({ req: values })
    );
  });

  it('should toggle password visibility on icon click', () => {
    const toggleButton = fixture.debugElement.query(By.css('.material-icons[matSuffix]')).nativeElement;
    toggleButton.click();
    fixture.detectChanges();

    expect(component.hideNewPassword).toBe(false);
  });

  it('should display validation error message if input is invalid', () => {
    mockValidationMessage.getValidationMessage.and.returnValue({ hasError: true, message: 'Invalid email' });

    component.formGroup.get('email')?.setErrors({ invalidEmail: true });

    const cdr = fixture.debugElement.injector.get<ChangeDetectorRef>(ChangeDetectorRef);
    cdr.detectChanges();

    const errorMessage = fixture.debugElement.query(By.css('mat-error')).nativeElement.textContent;
    expect(errorMessage).toContain('Invalid email');
  });

  it('should not dispatch updateUser if form is invalid', () => {
    component.formGroup.setErrors({ invalid: true });

    const saveButton = fixture.debugElement.query(By.css('button#send-button')).nativeElement;
    saveButton.click();

    expect(storeSpy.dispatch).not.toHaveBeenCalled();
  });

  it('should unsubscribe from destroy$ on component destruction', () => {
    const destroySpy = spyOn(component['destroy$'], 'next');
    const completeSpy = spyOn(component['destroy$'], 'complete');

    component.ngOnDestroy();

    expect(destroySpy).toHaveBeenCalled();
    expect(completeSpy).toHaveBeenCalled();
  });
});
