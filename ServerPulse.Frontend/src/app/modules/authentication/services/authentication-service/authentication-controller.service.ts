import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { getAuthData, logOutUser, refreshAccessToken, registerUser, selectAuthData, selectAuthErrors, selectIsRegistrationSuccess, selectRegistrationErrors, selectUpdateIsSuccessful, selectUserData, signInUser, updateUserData } from '../..';
import { AuthData, AuthToken, UserAuthenticationRequest, UserData, UserRegistrationRequest, UserUpdateDataRequest } from '../../../shared';
import { AuthenticationService } from './authentication-service';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationControllerService implements AuthenticationService {

  constructor(private store: Store) { }

  registerUser(userRegistrationData: UserRegistrationRequest): Observable<boolean> {
    this.store.dispatch(registerUser({ userRegistrationData: userRegistrationData }));
    return this.store.select(selectIsRegistrationSuccess);
  }
  getRegistrationErrors(): Observable<any> {
    return this.store.select(selectRegistrationErrors);
  }
  singInUser(userAuthData: UserAuthenticationRequest): Observable<AuthData> {
    this.store.dispatch(signInUser({ authData: userAuthData }));
    return this.store.select(selectAuthData);
  }
  getAuthData(): Observable<AuthData> {
    this.store.dispatch(getAuthData());
    return this.store.select(selectAuthData);
  }
  getAuthErrors(): Observable<any> {
    return this.store.select(selectAuthErrors);
  }
  getUserData(): Observable<UserData> {
    this.store.dispatch(getAuthData());
    return this.store.select(selectUserData);
  }
  logOutUser(): Observable<AuthData> {
    this.store.dispatch(logOutUser());
    return this.store.select(selectAuthData);
  }
  refreshToken(accessToken: AuthToken): Observable<AuthData> {
    this.store.dispatch(refreshAccessToken({ accessToken: accessToken }));
    return this.store.select(selectAuthData);
  }
  updateUser(updateData: UserUpdateDataRequest): Observable<boolean> {
    this.store.dispatch(updateUserData({ userUpdateData: updateData }));
    return this.store.select(selectUpdateIsSuccessful);
  }
}