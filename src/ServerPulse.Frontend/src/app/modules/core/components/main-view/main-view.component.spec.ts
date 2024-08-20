import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/compiler';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { RouterModule } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { AuthenticationDialogManager, AuthenticationService } from '../../../authentication';
import { AuthenticationModule } from '../../../authentication/authentication.module';
import { AuthData } from '../../../shared';
import { MainViewComponent } from './main-view.component';

describe('MainViewComponent', () => {
  const authData = {
    isAuthenticated: true,
    accessToken: "",
    refreshToken: "",
    refreshTokenExpiryDate: new Date(),
  };
  let component: MainViewComponent;
  let fixture: ComponentFixture<MainViewComponent>;
  let mockAuthService: jasmine.SpyObj<AuthenticationService>;
  let mockAuthDialogManager: jasmine.SpyObj<AuthenticationDialogManager>;
  let authDataBehabiourSubject = new BehaviorSubject<AuthData>(authData);

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj('AuthenticationService', ['getAuthData']);
    mockAuthDialogManager = jasmine.createSpyObj('AuthenticationDialogManager', ['openLoginMenu']);
    authDataBehabiourSubject = new BehaviorSubject<AuthData>(authData);
    mockAuthService.getAuthData.and.returnValue(authDataBehabiourSubject.asObservable());

    await TestBed.configureTestingModule({
      declarations: [MainViewComponent],
      imports: [RouterModule, AuthenticationModule],
      providers: [
        { provide: AuthenticationService, useValue: mockAuthService },
        { provide: AuthenticationDialogManager, useValue: mockAuthDialogManager }
      ],
      schemas: [CUSTOM_ELEMENTS_SCHEMA]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(MainViewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize isAuthenticated$ on ngOnInit', () => {
    component.ngOnInit();
    fixture.detectChanges();

    component.isAuthenticated$.subscribe(isAuthenticated => {
      expect(isAuthenticated).toBeTrue();
    });

    expect(mockAuthService.getAuthData).toHaveBeenCalled();
  });

  it('should call openLoginMenu when button is clicked', () => {
    const button = fixture.debugElement.nativeElement.querySelector('button[mat-icon-button]');
    button.click();

    expect(mockAuthDialogManager.openLoginMenu).toHaveBeenCalled();
  });

  it('should display authenticated view when user is authenticated', () => {
    component.ngOnInit();
    fixture.detectChanges();

    const authenticatedView = fixture.debugElement.nativeElement.querySelector('.root');
    expect(authenticatedView).toBeTruthy();
  });

  it('should display unauthenticated view when user is not authenticated', fakeAsync(() => {
    authDataBehabiourSubject.next({
      isAuthenticated: false,
      accessToken: "",
      refreshToken: "",
      refreshTokenExpiryDate: new Date(),
    });

    component.ngOnInit();
    tick();
    fixture.detectChanges();

    const unauthenticatedView = fixture.debugElement.nativeElement.querySelector('auth-unauthenticated');
    expect(unauthenticatedView).toBeTruthy();
  }));
});