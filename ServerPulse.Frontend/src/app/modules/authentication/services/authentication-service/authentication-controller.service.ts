import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { getAuthData, logOutUser, refreshAccessToken, registerUser, selectAuthData, selectAuthErrors, selectIsRegistrationSuccess, selectRegistrationErrors, selectUpdateIsSuccessful, selectUserData, selectUserErrors, signInUser, updateUserData } from '../..';
import { AuthData, AuthToken, UserAuthenticationRequest, UserData, UserRegistrationRequest, UserUpdateDataRequest } from '../../../shared';
import { AuthenticationService } from './authentication-service';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationControllerService implements AuthenticationService {

  constructor(
    private readonly store: Store
  ) { }

  //Registration
  registerUser(userRegistrationData: UserRegistrationRequest): Observable<boolean> {
    this.store.dispatch(registerUser({ registrationRequest: userRegistrationData }));
    return this.store.select(selectIsRegistrationSuccess);
  }
  getRegistrationErrors(): Observable<any> {
    return this.store.select(selectRegistrationErrors);
  }
  //Auth
  singInUser(userAuthData: UserAuthenticationRequest): Observable<AuthData> {
    this.store.dispatch(signInUser({ authRequest: userAuthData }));
    return this.store.select(selectAuthData);
  }
  getAuthData(): Observable<AuthData> {
    this.store.dispatch(getAuthData());
    return this.store.select(selectAuthData);
  }
  getAuthErrors(): Observable<any> {
    return this.store.select(selectAuthErrors);
  }
  logOutUser(): Observable<AuthData> {
    this.store.dispatch(logOutUser());
    return this.store.select(selectAuthData);
  }
  refreshToken(accessToken: AuthToken): Observable<AuthData> {
    this.store.dispatch(refreshAccessToken({ accessToken: accessToken }));
    return this.store.select(selectAuthData);
  }
  //User 
  getUserData(): Observable<UserData> {
    this.store.dispatch(getAuthData());
    return this.store.select(selectUserData);
  }
  updateUser(updateData: UserUpdateDataRequest): Observable<boolean> {
    this.store.dispatch(updateUserData({ updateRequest: updateData }));
    return this.store.select(selectUpdateIsSuccessful);
  }
  getUserErrors(): Observable<any> {
    return this.store.select(selectUserErrors);
  }
}