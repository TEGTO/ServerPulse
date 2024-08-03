import { TestBed } from '@angular/core/testing';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { getAuthData, logOutUser, refreshAccessToken, registerUser, selectAuthData, selectIsRefreshSuccessful, signInUser, updateUserData } from '../..';
import { AuthData, AuthToken, UserAuthenticationRequest, UserData, UserRegistrationRequest, UserUpdateDataRequest } from '../../../shared';
import { AuthenticationControllerService } from './authentication-controller.service';

describe('AuthenticationControllerService', () => {
  let service: AuthenticationControllerService;
  let store: jasmine.SpyObj<Store>;

  const mockAuthData: AuthData = {
    isAuthenticated: true,
    accessToken: 'authToken',
    refreshToken: 'refreshToken',
    refreshTokenExpiryDate: new Date()
  };

  const mockUserData: UserData = {
    userName: 'user',
    email: 'user@example.com'
  };

  beforeEach(() => {
    const storeSpy = jasmine.createSpyObj('Store', ['dispatch', 'select']);

    TestBed.configureTestingModule({
      providers: [
        AuthenticationControllerService,
        { provide: Store, useValue: storeSpy }
      ]
    });

    service = TestBed.inject(AuthenticationControllerService);
    store = TestBed.inject(Store) as jasmine.SpyObj<Store>;
    store.select.and.returnValue(of(mockAuthData));
  });

  it('should dispatch registerUser action and return isSuccess observable', (done) => {
    const userRegistrationData: UserRegistrationRequest = { userName: 'user', email: 'user@example.com', password: 'password', confirmPassword: 'password' };
    store.select.and.returnValue(of(true));

    service.registerUser(userRegistrationData).subscribe(result => {
      expect(store.dispatch).toHaveBeenCalledWith(registerUser({ registrationRequest: userRegistrationData }));
      expect(result).toBe(true);
      done();
    });
  });

  it('should return registration errors observable', (done) => {
    store.select.and.returnValue(of('Error'));

    service.getRegistrationErrors().subscribe(result => {
      expect(result).toBe('Error');
      done();
    });
  });

  it('should dispatch signInUser action and return authData observable', (done) => {
    const userAuthData: UserAuthenticationRequest = { login: 'user@example.com', password: 'password' };
    store.select.and.returnValue(of(mockAuthData));

    service.singInUser(userAuthData);
    service.getAuthData().subscribe(result => {
      expect(store.dispatch).toHaveBeenCalledWith(signInUser({ authRequest: userAuthData }));
      expect(result).toEqual(mockAuthData);
      done();
    });
  });

  it('should dispatch getAuthData action and return authData observable', (done) => {
    store.select.and.returnValue(of(mockAuthData));

    service.getAuthData().subscribe(result => {
      expect(store.dispatch).toHaveBeenCalledWith(getAuthData());
      expect(result).toEqual(mockAuthData);
      done();
    });
  });

  it('should return auth errors observable', (done) => {
    store.select.and.returnValue(of('Error'));

    service.getAuthErrors().subscribe(result => {
      expect(result).toBe('Error');
      done();
    });
  });

  it('should dispatch logOutUser action and return authData observable', (done) => {
    store.select.and.returnValue(of(mockAuthData));

    service.logOutUser();
    service.getAuthData().subscribe(result => {
      expect(store.dispatch).toHaveBeenCalledWith(logOutUser());
      expect(result).toEqual(mockAuthData);
      done();
    });
  });

  it('should dispatch refreshAccessToken action and return authData observable', (done) => {
    const accessToken: AuthToken = { accessToken: 'newToken', refreshToken: 'newRefresh', refreshTokenExpiryDate: new Date() };
    // @ts-ignore
    store.select.withArgs(selectIsRefreshSuccessful).and.returnValue(of(true));
    // @ts-ignore
    store.select.withArgs(selectAuthData).and.returnValue(of(mockAuthData));

    service.refreshToken(accessToken).subscribe(result => {
      expect(result).toEqual(true);
    });
    service.getAuthData().subscribe(result => {
      expect(store.dispatch).toHaveBeenCalledWith(refreshAccessToken({ authToken: accessToken }));
      expect(result).toEqual(mockAuthData);
      done();
    });
  });

  it('should dispatch getAuthData action and return userData observable', (done) => {
    store.select.and.returnValue(of(mockUserData));

    service.getUserData().subscribe(result => {
      expect(store.dispatch).toHaveBeenCalledWith(getAuthData());
      expect(result).toEqual(mockUserData);
      done();
    });
  });

  it('should dispatch updateUserData action and return isUpdateSuccessful observable', (done) => {
    const updateData: UserUpdateDataRequest = { userName: 'newUser', newEmail: 'new@example.com', oldEmail: 'user@example.com', oldPassword: 'oldPass', newPassword: 'newPass' };
    store.select.and.returnValue(of(true));

    service.updateUser(updateData).subscribe(result => {
      expect(store.dispatch).toHaveBeenCalledWith(updateUserData({ updateRequest: updateData }));
      expect(result).toBe(true);
      done();
    });
  });

  it('should return user errors observable', (done) => {
    store.select.and.returnValue(of('Error'));

    service.getUserErrors().subscribe(result => {
      expect(result).toBe('Error');
      done();
    });
  });
});