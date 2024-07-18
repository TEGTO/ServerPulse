import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TestBed } from "@angular/core/testing";
import { provideMockActions } from "@ngrx/effects/testing";
import { Observable, of, throwError } from "rxjs";
import { AuthData, AuthenticationApiService, AuthToken, LocalStorageService, UserAuthenticationRequest, UserAuthenticationResponse, UserData, UserUpdateDataRequest } from "../../../shared";
import { getAuthData, getAuthDataFailure, getAuthDataSuccess, logOutUser, logOutUserSuccess, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerFailure, registerSuccess, registerUser, signInUser, signInUserFailure, signInUserSuccess, updateUserData, updateUserDataFailure, updateUserDataSuccess } from "./auth.actions";
import { RegistrationEffects, SignInEffects } from "./auth.effects";

describe('RegistrationEffects', () => {
    let actions$: Observable<any>;
    let effects: RegistrationEffects;
    let mockApiService: jasmine.SpyObj<AuthenticationApiService>;

    beforeEach(() => {
        mockApiService = jasmine.createSpyObj('AuthenticationApiService', ['registerUser']);

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                RegistrationEffects,
                provideMockActions(() => actions$),
                { provide: AuthenticationApiService, useValue: mockApiService }
            ]
        });

        effects = TestBed.inject(RegistrationEffects);
    });

    describe('registerUser$', () => {
        it('should dispatch registerSuccess on successful registerUser', (done) => {
            const registrationRequest = { userName: 'user', email: 'user@example.com', password: 'password', confirmPassword: 'password' };
            const action = registerUser({ registrationRequest });
            const outcome = registerSuccess();

            actions$ = of(action);
            mockApiService.registerUser.and.returnValue(of({}));

            effects.registerUser$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.registerUser).toHaveBeenCalledWith(registrationRequest);
                done();
            });
        });

        it('should dispatch registerFailure on failed registerUser', (done) => {
            const registrationRequest = { userName: 'user', email: 'user@example.com', password: 'password', confirmPassword: 'password' };
            const action = registerUser({ registrationRequest });
            const error = new Error('Error!');
            const outcome = registerFailure({ error: error.message });

            actions$ = of(action);
            mockApiService.registerUser.and.returnValue(throwError(error));

            effects.registerUser$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.registerUser).toHaveBeenCalledWith(registrationRequest);
                done();
            });
        });
    });
});

describe('SignInEffects', () => {
    let actions$: Observable<any>;
    let effects: SignInEffects;
    let mockApiService: jasmine.SpyObj<AuthenticationApiService>;
    let mockLocalStorage: jasmine.SpyObj<LocalStorageService>;

    beforeEach(() => {
        mockApiService = jasmine.createSpyObj('AuthenticationApiService', ['loginUser', 'refreshToken', 'updateUser']);
        mockLocalStorage = jasmine.createSpyObj('LocalStorageService', ['setItem', 'getItem', 'removeItem']);

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                SignInEffects,
                provideMockActions(() => actions$),
                { provide: AuthenticationApiService, useValue: mockApiService },
                { provide: LocalStorageService, useValue: mockLocalStorage }
            ]
        });

        effects = TestBed.inject(SignInEffects);
    });

    describe('signInUser$', () => {
        it('should dispatch signInUserSuccess on successful signInUser', (done) => {
            const authRequest: UserAuthenticationRequest = { login: 'user@example.com', password: 'password' };
            const response: UserAuthenticationResponse = {
                authToken: { accessToken: 'accessToken', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() },
                userName: 'user',
                email: 'user@example.com'
            };
            const authData: AuthData = {
                isAuthenticated: true,
                accessToken: response.authToken.accessToken,
                refreshToken: response.authToken.refreshToken,
                refreshTokenExpiryDate: response.authToken.refreshTokenExpiryDate
            };
            const userData: UserData = { userName: response.userName, email: response.email };
            const action = signInUser({ authRequest });
            const outcome = signInUserSuccess({ authData, userData });

            actions$ = of(action);
            mockApiService.loginUser.and.returnValue(of(response));

            effects.singInUser$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.loginUser).toHaveBeenCalledWith(authRequest);
                expect(mockLocalStorage.setItem).toHaveBeenCalledWith('authData', JSON.stringify(authData));
                expect(mockLocalStorage.setItem).toHaveBeenCalledWith('userData', JSON.stringify(userData));
                done();
            });
        });

        it('should dispatch signInUserFailure on failed signInUser', (done) => {
            const authRequest: UserAuthenticationRequest = { login: 'user@example.com', password: 'password' };
            const action = signInUser({ authRequest });
            const error = new Error('Error!');
            const outcome = signInUserFailure({ error: error.message });

            actions$ = of(action);
            mockApiService.loginUser.and.returnValue(throwError(error));

            effects.singInUser$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.loginUser).toHaveBeenCalledWith(authRequest);
                done();
            });
        });
    });

    describe('getAuthUser$', () => {
        it('should dispatch getAuthDataSuccess if auth data is present in local storage', (done) => {
            const authData: AuthData = { isAuthenticated: true, accessToken: 'accessToken', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() };
            const userData: UserData = { userName: 'user', email: 'user@example.com' };
            const action = getAuthData();
            const outcome = getAuthDataSuccess({ authData, userData });

            mockLocalStorage.getItem.and.callFake((key: string) => {
                if (key === 'authData') return JSON.stringify(authData);
                if (key === 'userData') return JSON.stringify(userData);
                return null;
            });

            actions$ = of(action);

            effects.getAuthUser$.subscribe(result => {
                expect(result.type).toEqual(outcome.type);
                expect(mockLocalStorage.getItem).toHaveBeenCalledWith('authData');
                expect(mockLocalStorage.getItem).toHaveBeenCalledWith('userData');
                done();
            });
        });

        it('should dispatch getAuthDataFailure if no auth data is present in local storage', (done) => {
            const action = getAuthData();
            const outcome = getAuthDataFailure();

            mockLocalStorage.getItem.and.returnValue(null);

            actions$ = of(action);

            effects.getAuthUser$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockLocalStorage.getItem).toHaveBeenCalledWith('authData');
                expect(mockLocalStorage.getItem).toHaveBeenCalledWith('userData');
                done();
            });
        });
    });

    describe('logOutUser$', () => {
        it('should dispatch logOutUserSuccess and remove items from local storage', (done) => {
            const action = logOutUser();
            const outcome = logOutUserSuccess();

            mockLocalStorage.getItem.and.returnValue('someValue');

            actions$ = of(action);

            effects.logOutUser$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('authData');
                expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('userData');
                done();
            });
        });
    });

    describe('refreshToken$', () => {
        it('should dispatch refreshAccessTokenSuccess on successful refreshToken', (done) => {
            const authToken: AuthToken = { accessToken: 'newAccessToken', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() };
            const action = refreshAccessToken({ authToken: authToken });
            const outcome = refreshAccessTokenSuccess({ authToken: authToken });

            mockLocalStorage.getItem.and.returnValue(JSON.stringify({ isAuthenticated: true, accessToken: 'oldAccessToken', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() }));

            actions$ = of(action);
            mockApiService.refreshToken.and.returnValue(of(authToken));

            effects.refreshToken$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.refreshToken).toHaveBeenCalledWith(authToken);
                expect(mockLocalStorage.getItem).toHaveBeenCalledWith('authData');
                expect(mockLocalStorage.setItem).toHaveBeenCalledWith('authData', JSON.stringify({ isAuthenticated: true, accessToken: 'newAccessToken', refreshToken: 'refreshToken', refreshTokenExpiryDate: authToken.refreshTokenExpiryDate }));
                done();
            });
        });

        it('should dispatch refreshAccessTokenFailure on failed refreshToken', (done) => {
            const authToken: AuthToken = { accessToken: 'newAccessToken', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() };
            const action = refreshAccessToken({ authToken: authToken });
            const error = new Error('Error!');
            const outcome = refreshAccessTokenFailure({ error: error.message });

            actions$ = of(action);
            mockApiService.refreshToken.and.returnValue(throwError(error));

            effects.refreshToken$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.refreshToken).toHaveBeenCalledWith(authToken);
                expect(mockLocalStorage.removeItem).toHaveBeenCalledWith('authData');
                done();
            });
        });
    });

    describe('updateUser$', () => {
        it('should dispatch updateUserDataSuccess on successful updateUser', (done) => {
            const updateRequest: UserUpdateDataRequest = {
                userName: 'newUser',
                newEmail: 'newUser@example.com',
                oldEmail: 'user@example.com',
                oldPassword: 'oldPassword',
                newPassword: 'newPassword'
            };
            const userData: UserData = { userName: 'newUser', email: 'newUser@example.com' };
            const action = updateUserData({ updateRequest });
            const outcome = updateUserDataSuccess({ userData });

            actions$ = of(action);
            mockApiService.updateUser.and.returnValue(of({}));

            effects.updateUser$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.updateUser).toHaveBeenCalledWith(updateRequest);
                expect(mockLocalStorage.setItem).toHaveBeenCalledWith('userData', JSON.stringify(userData));
                done();
            });
        });

        it('should dispatch updateUserDataFailure on failed updateUser', (done) => {
            const updateRequest: UserUpdateDataRequest = {
                userName: 'newUser',
                newEmail: 'newUser@example.com',
                oldEmail: 'user@example.com',
                oldPassword: 'oldPassword',
                newPassword: 'newPassword'
            };
            const action = updateUserData({ updateRequest });
            const error = new Error('Error!');
            const outcome = updateUserDataFailure({ error: error.message });

            actions$ = of(action);
            mockApiService.updateUser.and.returnValue(throwError(error));

            effects.updateUser$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.updateUser).toHaveBeenCalledWith(updateRequest);
                done();
            });
        });
    });
});