import { ComponentFixture, TestBed } from '@angular/core/testing';

import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { AuthenticationDialogManager } from '../..';
import { UnauthenticatedComponent } from './unauthenticated.component';

describe('UnauthenticatedComponent', () => {
  let component: UnauthenticatedComponent;
  let fixture: ComponentFixture<UnauthenticatedComponent>;
  let authDialogManager: jasmine.SpyObj<AuthenticationDialogManager>;

  beforeEach(async () => {
    const authDialogManagerSpy = jasmine.createSpyObj('AuthenticationDialogManager', ['openLoginMenu']);

    await TestBed.configureTestingModule({
      declarations: [UnauthenticatedComponent],
      imports: [NoopAnimationsModule],
      providers: [
        { provide: AuthenticationDialogManager, useValue: authDialogManagerSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UnauthenticatedComponent);
    component = fixture.componentInstance;
    authDialogManager = TestBed.inject(AuthenticationDialogManager) as jasmine.SpyObj<AuthenticationDialogManager>;

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should call openLoginMenu when button is clicked', () => {
    const button = fixture.debugElement.query(By.css('button')).nativeElement;
    button.click();

    expect(authDialogManager.openLoginMenu).toHaveBeenCalled();
  });
});