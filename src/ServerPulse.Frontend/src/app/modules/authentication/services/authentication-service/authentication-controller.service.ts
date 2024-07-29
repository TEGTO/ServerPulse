import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { getAuthData, logOutUser, refreshAccessToken, registerUser, selectAuthData, selectAuthErrors, selectIsRefreshSuccessful, selectIsRegistrationSuccess, selectIsUpdateSuccessful, selectRegistrationErrors, selectUserData, selectUserErrors, signInUser, updateUserData } from '../..';
import { AuthData, AuthToken, UserAuthenticationRequest, UserData, UserRegistrationRequest, UserUpdateDataRequest } from '../../../shared';
import { AuthenticationService } from './authentication-service';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationControllerService implements AuthenticationService {

  constructor(
    private readonly store: Store,
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
  getAuthData(): Observable<AuthData> {
    this.store.dispatch(getAuthData());
    return this.store.select(selectAuthData);
  }
  getAuthErrors(): Observable<any> {
    return this.store.select(selectAuthErrors);
  }
  singInUser(authRequest: UserAuthenticationRequest): void {
    this.store.dispatch(signInUser({ authRequest: authRequest }));
  }
  logOutUser(): void {
    this.store.dispatch(logOutUser());
  }
  refreshToken(authToken: AuthToken): Observable<boolean> {
    this.store.dispatch(refreshAccessToken({ authToken: authToken }));
    return this.store.select(selectIsRefreshSuccessful);
  }
  //User 
  getUserData(): Observable<UserData> {
    this.store.dispatch(getAuthData());
    return this.store.select(selectUserData);
  }
  updateUser(updateData: UserUpdateDataRequest): Observable<boolean> {
    this.store.dispatch(updateUserData({ updateRequest: updateData }));
    return this.store.select(selectIsUpdateSuccessful);
  }
  getUserErrors(): Observable<any> {
    return this.store.select(selectUserErrors);
  }
}