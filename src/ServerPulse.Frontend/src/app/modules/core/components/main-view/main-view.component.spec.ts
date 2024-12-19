import { CUSTOM_ELEMENTS_SCHEMA } from "@angular/core";
import { ComponentFixture, TestBed, fakeAsync, tick } from "@angular/core/testing";
import { RouterModule } from "@angular/router";
import { EffectsModule } from "@ngrx/effects";
import { Store, StoreModule } from "@ngrx/store";
import { BehaviorSubject } from "rxjs";
import { MainViewComponent } from "../..";
import { AuthData, getAuthData, getDefaultAuthToken, startLoginUser } from "../../../authentication";
import { AuthenticationModule } from "../../../authentication/authentication.module";

describe('MainViewComponent', () => {
  const authData: AuthData = {
    isAuthenticated: true,
    authToken: getDefaultAuthToken(),
    email: "someemail@gmail.com"
  };

  let component: MainViewComponent;
  let fixture: ComponentFixture<MainViewComponent>;
  let storeSpy: jasmine.SpyObj<Store>;
  let authDataBehabiourSubject = new BehaviorSubject<AuthData>(authData);

  beforeEach(async () => {
    storeSpy = jasmine.createSpyObj<Store>(['dispatch', 'select']);
    authDataBehabiourSubject = new BehaviorSubject<AuthData>(authData);

    storeSpy.select.and.returnValue(authDataBehabiourSubject.asObservable());

    await TestBed.configureTestingModule({
      declarations: [MainViewComponent],
      imports: [
        RouterModule.forRoot([]),
        StoreModule.forRoot({}, { runtimeChecks: { strictStateImmutability: true, strictActionImmutability: true } }),
        EffectsModule.forRoot([]),
        AuthenticationModule
      ],
      providers: [
        { provide: Store, useValue: storeSpy },
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

    expect(storeSpy.dispatch).toHaveBeenCalledWith(getAuthData());
  });

  it('should call startLogin when button is clicked', () => {
    const button = fixture.debugElement.nativeElement.querySelector('button[mat-icon-button]');
    button.click();

    expect(storeSpy.dispatch).toHaveBeenCalledWith(startLoginUser());
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
      authToken: getDefaultAuthToken(),
      email: ""
    });

    component.ngOnInit();
    tick();
    fixture.detectChanges();

    const unauthenticatedView = fixture.debugElement.nativeElement.querySelector('app-unauthenticated');
    expect(unauthenticatedView).toBeTruthy();
  }));
});