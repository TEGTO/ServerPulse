/* eslint-disable @typescript-eslint/no-explicit-any */
import { HttpResponse } from '@angular/common/http';
import { fakeAsync, TestBed, tick } from '@angular/core/testing';
import { provideMockActions } from '@ngrx/effects/testing';
import { Store } from '@ngrx/store';
import { BehaviorSubject, Observable, of, throwError } from 'rxjs';
import { AuthData, AuthenticationApiService, AuthenticationDialogManagerService, authFailure, AuthToken, confirmEmail, EmailConfirmationRequest, getAuthData, getAuthDataFailure, getAuthDataSuccess, loginUser, loginUserSuccess, logOutUser, logOutUserSuccess, OauthApiService, oauthLogin, oauthLoginFailure, OAuthLoginProvider, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerUser, startLoginUser, startOAuthLogin, startOAuthLoginFailure, startRegisterUser, updateUserData, updateUserDataSuccess, UserAuthenticationRequest, UserRegistrationRequest } from '..';
import { LocalStorageService, RedirectorService, SnackbarManager } from '../../shared';
import { AuthEffects } from './auth.effects';

describe('AuthEffects', () => {
    let actions$: Observable<any>;
    let effects: AuthEffects;

    let storeSpy: jasmine.SpyObj<Store>;
    let authApiServiceSpy: jasmine.SpyObj<AuthenticationApiService>;
    let oauthApiServiceSpy: jasmine.SpyObj<OauthApiService>;
    let localStorageSpy: jasmine.SpyObj<LocalStorageService>;
    let redirectorSpy: jasmine.SpyObj<RedirectorService>;
    let snackbarManagerSpy: jasmine.SpyObj<SnackbarManager>;
    let dialogManagerSpy: jasmine.SpyObj<AuthenticationDialogManagerService>;
    let storeSelect$: BehaviorSubject<any>;

    beforeEach(() => {
        storeSpy = jasmine.createSpyObj<Store>(['select', 'dispatch']);
        authApiServiceSpy = jasmine.createSpyObj<AuthenticationApiService>(
            ['registerUser', 'loginUser', 'refreshToken', 'updateUser', 'confirmEmail']
        );
        oauthApiServiceSpy = jasmine.createSpyObj<OauthApiService>(['loginUserOAuth', 'getOAuthUrl']);
        localStorageSpy = jasmine.createSpyObj<LocalStorageService>(['getItem', 'setItem', 'removeItem']);
        redirectorSpy = jasmine.createSpyObj<RedirectorService>(['redirectToHome', 'redirectToExternalUrl']);
        snackbarManagerSpy = jasmine.createSpyObj<SnackbarManager>(['openInfoSnackbar', 'openErrorSnackbar']);
        dialogManagerSpy = jasmine.createSpyObj<AuthenticationDialogManagerService>(
            ['openRegisterMenu', 'openLoginMenu', 'openAuthenticatedMenu', 'closeAll']
        );

        storeSelect$ = new BehaviorSubject({ authData: { isAuthenticated: true } });
        storeSpy.select.and.returnValue(storeSelect$.asObservable());

        TestBed.configureTestingModule({
            providers: [
                AuthEffects,
                provideMockActions(() => actions$),
                { provide: Store, useValue: storeSpy },
                { provide: AuthenticationApiService, useValue: authApiServiceSpy },
                { provide: OauthApiService, useValue: oauthApiServiceSpy },
                { provide: LocalStorageService, useValue: localStorageSpy },
                { provide: RedirectorService, useValue: redirectorSpy },
                { provide: SnackbarManager, useValue: snackbarManagerSpy },
                { provide: AuthenticationDialogManagerService, useValue: dialogManagerSpy },
            ],
        });

        effects = TestBed.inject(AuthEffects);
    });

    describe('Registration', () => {
        it('should call dialogManager.openRegisterMenu on startRegisterUser$', fakeAsync(() => {
            actions$ = of(startRegisterUser());

            effects.startRegisterUser$.subscribe();
            tick();
            expect(dialogManagerSpy.openRegisterMenu).toHaveBeenCalled();
        }));

        it('should call registerUser API and handle success', fakeAsync(() => {
            const req: UserRegistrationRequest = {
                email: 'test@example.com',
                password: 'password',
                confirmPassword: 'password',
                redirectConfirmUrl: 'http://example.com/confirm',
            };

            authApiServiceSpy.registerUser.and.returnValue(of(new HttpResponse<void>()));

            actions$ = of(registerUser({ req }));

            effects.registerUser$.subscribe();
            tick(1000);
            expect(authApiServiceSpy.registerUser).toHaveBeenCalledWith(req);
            expect(snackbarManagerSpy.openInfoSnackbar).toHaveBeenCalledWith(
                '✔️ The registration is successful! Please confirm the email!',
                15
            );
            expect(dialogManagerSpy.closeAll).toHaveBeenCalled();
            expect(redirectorSpy.redirectToHome).toHaveBeenCalled();
        }));

        it('should call registerUser API and handle error', () => {
            const req: UserRegistrationRequest = {
                email: 'test@example.com',
                password: 'password',
                confirmPassword: 'password',
                redirectConfirmUrl: 'http://example.com/confirm',
            };
            const error = { message: 'Registration failed' };

            authApiServiceSpy.registerUser.and.returnValue(throwError(() => error));

            actions$ = of(registerUser({ req }));

            effects.registerUser$.subscribe((action) => {
                expect(action).toEqual(authFailure({ error: error.message }));
                expect(authApiServiceSpy.registerUser).toHaveBeenCalledWith(req);
            });
        });

    })

    describe('Confirmation', () => {
        it('should dispatch loginUserSuccess on successful confirmEmail', () => {
            const req: EmailConfirmationRequest = {
                email: 'test@example.com',
                token: 'confirmationToken'
            };
            const authData: AuthData = {
                isAuthenticated: true,
                authToken: { accessToken: 'accessToken', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() },
                email: 'test@example.com'
            };

            authApiServiceSpy.confirmEmail.and.returnValue(of(authData));
            actions$ = of(confirmEmail({ req }));

            effects.confirmEmail$.subscribe((action) => {
                expect(action).toEqual(loginUserSuccess({ authData }));
                expect(authApiServiceSpy.confirmEmail).toHaveBeenCalledWith(req);
                expect(localStorageSpy.setItem).toHaveBeenCalledWith(effects["storageAuthDataKey"], JSON.stringify(authData));
                expect(dialogManagerSpy.closeAll).toHaveBeenCalled();
                expect(redirectorSpy.redirectToHome).toHaveBeenCalled();
            });
        });

        it('should dispatch authFailure on confirmEmail error', () => {
            const req: EmailConfirmationRequest = {
                email: 'test@example.com',
                token: 'invalidToken'
            };
            const error = { message: 'Invalid confirmation token' };

            authApiServiceSpy.confirmEmail.and.returnValue(throwError(() => error));
            actions$ = of(confirmEmail({ req }));

            effects.confirmEmail$.subscribe((action) => {
                expect(action).toEqual(authFailure({ error: error.message }));
                expect(authApiServiceSpy.confirmEmail).toHaveBeenCalledWith(req);
                expect(localStorageSpy.setItem).not.toHaveBeenCalled();
                expect(dialogManagerSpy.closeAll).not.toHaveBeenCalled();
                expect(redirectorSpy.redirectToHome).not.toHaveBeenCalled();
            });
        });
    })

    describe('Start Login', () => {
        it('should openAuthenticatedMenu if user is authenticated', fakeAsync(() => {
            storeSelect$.next({ authData: { isAuthenticated: true } });

            actions$ = of(startLoginUser());

            effects.startLoginUser$.subscribe();
            tick();
            expect(dialogManagerSpy.openAuthenticatedMenu).toHaveBeenCalled();
            expect(dialogManagerSpy.openLoginMenu).not.toHaveBeenCalled();
        }));

        it('should openLoginMenu if user is not authenticated', fakeAsync(() => {
            storeSelect$.next({ authData: { isAuthenticated: false } });

            actions$ = of(startLoginUser());

            effects.startLoginUser$.subscribe();
            tick();
            expect(dialogManagerSpy.openLoginMenu).toHaveBeenCalled();
            expect(dialogManagerSpy.openAuthenticatedMenu).not.toHaveBeenCalled();
        }));
    })

    describe('Login', () => {
        it('should call loginUser API and handle success', () => {
            const req: UserAuthenticationRequest = { login: 'test@example.com', password: 'password' };
            const authData: AuthData = {
                isAuthenticated: true,
                authToken: { accessToken: 'access', refreshToken: 'refresh', refreshTokenExpiryDate: new Date() },
                email: 'test@example.com',
            };

            authApiServiceSpy.loginUser.and.returnValue(of(authData));

            actions$ = of(loginUser({ req }));

            effects.loginUser$.subscribe((action) => {
                expect(action).toEqual(loginUserSuccess({ authData }));
                expect(authApiServiceSpy.loginUser).toHaveBeenCalledWith(req);
                expect(localStorageSpy.setItem).toHaveBeenCalledWith('authData', JSON.stringify(authData));
                expect(dialogManagerSpy.closeAll).toHaveBeenCalled();
                expect(redirectorSpy.redirectToHome).toHaveBeenCalled();
            });
        });

        it('should handle loginUser$ error', () => {
            const req: UserAuthenticationRequest = { login: 'test@example.com', password: 'password' };
            const error = { message: 'Login failed' };

            authApiServiceSpy.loginUser.and.returnValue(throwError(() => error));

            actions$ = of(loginUser({ req }));

            effects.loginUser$.subscribe((action) => {
                expect(action).toEqual(authFailure({ error: error.message }));
                expect(authApiServiceSpy.loginUser).toHaveBeenCalledWith(req);
            });
        });
    })

    describe('Refresh Token', () => {
        it('should refresh token and update auth data', () => {
            const authToken: AuthToken = {
                accessToken: 'newAccessToken',
                refreshToken: 'newRefreshToken',
                refreshTokenExpiryDate: new Date(),
            };

            authApiServiceSpy.refreshToken.and.returnValue(of(authToken));
            localStorageSpy.getItem.and.returnValue(
                JSON.stringify({
                    isAuthenticated: true,
                    authToken: { accessToken: 'oldAccessToken', refreshToken: 'oldRefreshToken' },
                    email: 'test@example.com',
                })
            );

            actions$ = of(refreshAccessToken({ authToken }));

            effects.refreshToken$.subscribe((action) => {
                expect(action).toEqual(refreshAccessTokenSuccess({ authToken }));
                expect(authApiServiceSpy.refreshToken).toHaveBeenCalledWith(authToken);
                expect(localStorageSpy.setItem).toHaveBeenCalledWith(
                    'authData',
                    jasmine.any(String)
                );
            });
        });

        it('should handle refreshToken$ error and clear auth data', () => {
            const authToken: AuthToken = {
                accessToken: 'newAccessToken',
                refreshToken: 'newRefreshToken',
                refreshTokenExpiryDate: new Date(),
            };
            const error = { message: 'Refresh failed' };

            authApiServiceSpy.refreshToken.and.returnValue(throwError(() => error));

            actions$ = of(refreshAccessToken({ authToken }));

            effects.refreshToken$.subscribe((action) => {
                expect(action).toEqual(refreshAccessTokenFailure({ error: error.message }));
                expect(localStorageSpy.removeItem).toHaveBeenCalledWith('authData');
            });
        });
    })

    describe('Log Out', () => {
        it('should log out user and clear localStorage', () => {
            actions$ = of(logOutUser());

            effects.logOutUser$.subscribe((action) => {
                expect(action).toEqual(logOutUserSuccess());
                expect(localStorageSpy.removeItem).toHaveBeenCalledWith('authData');
                expect(redirectorSpy.redirectToHome).toHaveBeenCalled();
            });
        });
    })

    describe('Get Auth Data', () => {
        it('should handle getAuthData with success', () => {
            const authData: AuthData = {
                isAuthenticated: true,
                authToken: { accessToken: 'access', refreshToken: 'refresh', refreshTokenExpiryDate: new Date(0) },
                email: 'test@example.com',
            };

            localStorageSpy.getItem.and.returnValue(JSON.stringify(authData));

            actions$ = of(getAuthData());

            effects.getAuthData$.subscribe((action) => {

                expect(action).toEqual(jasmine.objectContaining({
                    type: getAuthDataSuccess.type
                }));
                expect(localStorageSpy.getItem).toHaveBeenCalledWith('authData');
            });
        });

        it('should handle getAuthData failure', () => {
            localStorageSpy.getItem.and.returnValue(null);

            actions$ = of(getAuthData());

            effects.getAuthData$.subscribe((action) => {
                expect(action).toEqual(getAuthDataFailure());
            });
        });
    })

    describe('Update User', () => {
        it('should dispatch updateUserDataSuccess on successful updateUser API call', () => {
            const req = { email: 'test@example.com', password: 'newpassword', oldPassword: 'oldpassword' };
            const authData: AuthData = {
                isAuthenticated: true,
                authToken: { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() },
                email: 'test@example.com',
            };

            localStorageSpy.getItem.and.returnValue(JSON.stringify(authData));
            authApiServiceSpy.updateUser.and.returnValue(of(new HttpResponse<void>()));
            actions$ = of(updateUserData({ req }));

            effects.updateUserData$.subscribe((action) => {
                expect(action).toEqual(updateUserDataSuccess({ req }));
                expect(authApiServiceSpy.updateUser).toHaveBeenCalledWith(req);
                expect(localStorageSpy.setItem).toHaveBeenCalledWith('authData', jasmine.any(String));
                expect(snackbarManagerSpy.openInfoSnackbar).toHaveBeenCalledWith('✔️ The update is successful!', 5);
                expect(dialogManagerSpy.closeAll).toHaveBeenCalled();
            });
        });

        it('should dispatch authFailure on API error', () => {
            const req = { email: 'test@example.com', password: 'newpassword', oldPassword: 'oldpassword' };
            const error = { message: 'Update failed' };
            authApiServiceSpy.updateUser.and.returnValue(throwError(() => error));
            actions$ = of(updateUserData({ req }));

            effects.updateUserData$.subscribe((action) => {
                expect(action).toEqual(authFailure({ error: error.message }));
                expect(authApiServiceSpy.updateUser).toHaveBeenCalledWith(req);
            });
        });
    });

    describe('OAuth Login', () => {
        it('should dispatch loginUserSuccess on successful OAuth login', () => {
            const code = 'sampleCode';
            const oauthParams = {
                codeVerifier: 'codeVerifier',
                redirectUrl: 'redirectUrl',
                oAuthLoginProvider: OAuthLoginProvider.Google,
            };
            const authData: AuthData = {
                isAuthenticated: true,
                authToken: { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() },
                email: 'test@example.com',
            };

            localStorageSpy.getItem.and.returnValue(JSON.stringify(oauthParams));
            oauthApiServiceSpy.loginUserOAuth.and.returnValue(of(authData));
            actions$ = of(oauthLogin({ code }));

            effects.oauthLogin$.subscribe((action) => {
                expect(action).toEqual(loginUserSuccess({ authData }));
                expect(oauthApiServiceSpy.loginUserOAuth).toHaveBeenCalledWith({
                    code,
                    codeVerifier: oauthParams.codeVerifier,
                    redirectUrl: oauthParams.redirectUrl,
                    oAuthLoginProvider: oauthParams.oAuthLoginProvider,
                });
                expect(localStorageSpy.setItem).toHaveBeenCalledWith('authData', JSON.stringify(authData));
            });
        });

        it('should dispatch oauthLoginFailure on missing OAuth params', () => {
            localStorageSpy.getItem.and.returnValue(null);
            actions$ = of(oauthLogin({ code: 'sampleCode' }));

            effects.oauthLogin$.subscribe((action) => {
                expect(action).toEqual(oauthLoginFailure({ error: new Error('Failed to get oauth url params!') }));
            });
        });

        it('should dispatch authFailure on API error', () => {
            const code = 'sampleCode';
            const oauthParams = {
                codeVerifier: 'codeVerifier',
                redirectUrl: 'redirectUrl',
                oAuthLoginProvider: 'Google',
            };
            const error = { message: 'OAuth login failed' };

            localStorageSpy.getItem.and.returnValue(JSON.stringify(oauthParams));
            oauthApiServiceSpy.loginUserOAuth.and.returnValue(throwError(() => error));
            actions$ = of(oauthLogin({ code }));

            effects.oauthLogin$.subscribe((action) => {
                expect(action).toEqual(authFailure({ error: error.message }));
            });
        });

        it('should call snackbarManager.openErrorSnackbar', () => {
            const error = 'Some error occurred';
            actions$ = of(oauthLoginFailure({ error }));

            effects.oauthLoginFailure$.subscribe(() => {
                expect(snackbarManagerSpy.openErrorSnackbar).toHaveBeenCalledWith(['OAuth login failed: ' + error]);
            });
        });
    });

    describe('Start OAuth Login', () => {
        it('should redirect to external URL on successful OAuth URL fetch', () => {
            const loginProvider = OAuthLoginProvider.Google;
            const oauthUrl = { url: 'https://oauth.com/auth' };
            oauthApiServiceSpy.getOAuthUrl.and.returnValue(of(oauthUrl));
            actions$ = of(startOAuthLogin({ loginProvider }));

            effects.startOAuthLogin$.subscribe(() => {
                expect(redirectorSpy.redirectToExternalUrl).toHaveBeenCalledWith(oauthUrl.url);
                expect(localStorageSpy.setItem).toHaveBeenCalledWith(
                    'OAuthParams',
                    jasmine.any(String)
                );
            });
        });

        it('should dispatch startOAuthLoginFailure on API error', () => {
            const loginProvider = OAuthLoginProvider.Google;
            const error = { message: 'OAuth URL fetch failed' };
            oauthApiServiceSpy.getOAuthUrl.and.returnValue(throwError(() => error));
            actions$ = of(startOAuthLogin({ loginProvider }));

            effects.startOAuthLogin$.subscribe((action) => {
                expect(action).toEqual(startOAuthLoginFailure({ error }));
            });
        });

        it('should call snackbarManager.openErrorSnackbar', () => {
            const error = 'Some error occurred';
            actions$ = of(startOAuthLoginFailure({ error }));

            effects.startOAuthLoginFailure$.subscribe(() => {
                expect(snackbarManagerSpy.openErrorSnackbar).toHaveBeenCalledWith(['Failed to get oauth url: ' + error]);
            });
        });
    });
});
