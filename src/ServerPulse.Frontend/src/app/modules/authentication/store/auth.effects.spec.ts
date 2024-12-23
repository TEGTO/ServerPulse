/* eslint-disable @typescript-eslint/no-explicit-any */
import { TestBed } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Store } from '@ngrx/store';
import { Observable, of, throwError } from 'rxjs';
import { AuthData, AuthenticationApiService, AuthenticationDialogManager, getAuthData, getAuthDataFailure, getAuthDataSuccess, loginUser, loginUserFailure, loginUserSuccess, logOutUser, logOutUserSuccess, registerFailure, registerSuccess, registerUser, startRegisterUser, UserAuthenticationRequest, UserRegistrationRequest } from '..';
import { LocalStorageService, RedirectorService, SnackbarManager } from '../../shared';
import { AuthEffects } from './auth.effects';

describe('AuthEffects', () => {
    let actions$: Observable<any>;
    let effects: AuthEffects;

    let storeSpy: jasmine.SpyObj<Store>;
    let authApiServiceSpy: jasmine.SpyObj<AuthenticationApiService>;
    let localStorageSpy: jasmine.SpyObj<LocalStorageService>;
    let redirectorSpy: jasmine.SpyObj<RedirectorService>;
    let snackbarManagerSpy: jasmine.SpyObj<SnackbarManager>;
    let dialogManagerSpy: jasmine.SpyObj<AuthenticationDialogManager>;

    beforeEach(() => {
        storeSpy = jasmine.createSpyObj<Store>(['select', 'dispatch']);
        authApiServiceSpy = jasmine.createSpyObj<AuthenticationApiService>(['registerUser', 'loginUser', 'refreshToken', 'updateUser']);
        localStorageSpy = jasmine.createSpyObj<LocalStorageService>(['getItem', 'setItem', 'removeItem']);
        redirectorSpy = jasmine.createSpyObj<RedirectorService>(['redirectToHome']);
        snackbarManagerSpy = jasmine.createSpyObj<SnackbarManager>(['openInfoSnackbar']);
        dialogManagerSpy = jasmine.createSpyObj<AuthenticationDialogManager>(['openRegisterMenu', 'openLoginMenu', 'closeAll']);

        TestBed.configureTestingModule({
            providers: [
                AuthEffects,
                provideMockActions(() => actions$),
                { provide: Store, useValue: storeSpy },
                { provide: AuthenticationApiService, useValue: authApiServiceSpy },
                { provide: LocalStorageService, useValue: localStorageSpy },
                { provide: RedirectorService, useValue: redirectorSpy },
                { provide: SnackbarManager, useValue: snackbarManagerSpy },
                { provide: AuthenticationDialogManager, useValue: dialogManagerSpy },
            ],
        });

        effects = TestBed.inject(AuthEffects);
    });

    it('should call dialogManager.openRegisterMenu on startRegisterUser$', () => {
        actions$ = of(startRegisterUser());

        effects.startRegisterUser$.subscribe(() => {
            expect(dialogManagerSpy.openRegisterMenu).toHaveBeenCalled();
        });
    });

    it('should dispatch registerSuccess on successful registerUser$', () => {
        const req: UserRegistrationRequest = { email: 'test@example.com', password: 'password123', confirmPassword: 'password123' };
        const authData: AuthData = { isAuthenticated: true, authToken: { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() }, email: 'test@example.com' };
        authApiServiceSpy.registerUser.and.returnValue(of(authData));

        actions$ = of(registerUser({ req }));

        effects.registerUser$.subscribe((action) => {
            expect(action).toEqual(registerSuccess({ authData }));
            expect(authApiServiceSpy.registerUser).toHaveBeenCalledWith(req);
            expect(localStorageSpy.setItem).toHaveBeenCalledWith('authData', JSON.stringify(authData));
            expect(snackbarManagerSpy.openInfoSnackbar).toHaveBeenCalledWith('✔️ The registration is successful!', 5);
        });
    });

    it('should dispatch registerFailure on registerUser$ error', () => {
        const req: UserRegistrationRequest = { email: 'test@example.com', password: 'password123', confirmPassword: 'password123' };
        const error = { message: 'Registration failed' };
        authApiServiceSpy.registerUser.and.returnValue(throwError(() => error));

        actions$ = of(registerUser({ req }));

        effects.registerUser$.subscribe((action) => {
            expect(action).toEqual(registerFailure({ error: error.message }));
            expect(authApiServiceSpy.registerUser).toHaveBeenCalledWith(req);
        });
    });

    it('should dispatch loginUserSuccess on successful loginUser$', () => {
        const req: UserAuthenticationRequest = { login: 'test@example.com', password: 'password123' };
        const authData: AuthData = { isAuthenticated: true, authToken: { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() }, email: 'test@example.com' };
        authApiServiceSpy.loginUser.and.returnValue(of(authData));

        actions$ = of(loginUser({ req }));

        effects.loginUser$.subscribe((action) => {
            expect(action).toEqual(loginUserSuccess({ authData }));
            expect(authApiServiceSpy.loginUser).toHaveBeenCalledWith(req);
            expect(localStorageSpy.setItem).toHaveBeenCalledWith('authData', JSON.stringify(authData));
        });
    });

    it('should dispatch loginUserFailure on loginUser$ error', () => {
        const req: UserAuthenticationRequest = { login: 'test@example.com', password: 'password123' };
        const error = { message: 'Login failed' };
        authApiServiceSpy.loginUser.and.returnValue(throwError(() => error));

        actions$ = of(loginUser({ req }));

        effects.loginUser$.subscribe((action) => {
            expect(action).toEqual(loginUserFailure({ error: error.message }));
            expect(authApiServiceSpy.loginUser).toHaveBeenCalledWith(req);
        });
    });

    it('should dispatch getAuthDataSuccess when auth data exists in localStorage', () => {
        const authData: AuthData = { isAuthenticated: true, authToken: { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date(0) }, email: 'test@example.com' };
        localStorageSpy.getItem.and.returnValue(JSON.stringify(authData));

        actions$ = of(getAuthData());

        effects.getAuthData$.subscribe((action) => {
            expect(action.type).toEqual(getAuthDataSuccess({ authData }).type);
            expect(localStorageSpy.getItem).toHaveBeenCalledWith(effects.storageAuthDataKey);
        });
    });

    it('should dispatch getAuthDataFailure when auth data does not exist in localStorage', () => {
        localStorageSpy.getItem.and.returnValue(null);

        actions$ = of(getAuthData());

        effects.getAuthData$.subscribe((action) => {
            expect(action).toEqual(getAuthDataFailure());
        });
    });

    it('should dispatch logOutUserSuccess and remove auth data from localStorage on logOutUser$', () => {
        actions$ = of(logOutUser());

        effects.logOutUser$.subscribe((action) => {
            expect(action).toEqual(logOutUserSuccess());
            expect(localStorageSpy.removeItem).toHaveBeenCalledWith('authData');
        });
    });
});
