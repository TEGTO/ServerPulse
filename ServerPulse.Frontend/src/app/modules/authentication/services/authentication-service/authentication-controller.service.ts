import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { getAuthUserData, logOutUser, refreshAccessToken, registerUser, selectAuthData, selectAuthErrors, selectIsRegistrationSuccess, selectRegistrationErrors, selectUpdateIsSuccessful, signInUser, updateUserData } from '../..';
import { AccessTokenDto, UserAuthData, UserAuthenticationRequest, UserRegistrationRequest, UserUpdateDataRequest } from '../../../shared';
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
  registerUserGetErrors(): Observable<any> {
    return this.store.select(selectRegistrationErrors);
  }
  singInUser(userAuthData: UserAuthenticationRequest): Observable<UserAuthData> {
    this.store.dispatch(signInUser({ userAuthData: userAuthData }));
    return this.store.select(selectAuthData);
  }
  getAuthUserData(): Observable<UserAuthData> {
    this.store.dispatch(getAuthUserData());
    return this.store.select(selectAuthData);
  }
  logOutUser(): Observable<UserAuthData> {
    this.store.dispatch(logOutUser());
    return this.store.select(selectAuthData);
  }
  refreshToken(accessToken: AccessTokenDto): Observable<UserAuthData> {
    this.store.dispatch(refreshAccessToken({ accessToken: accessToken }));
    return this.store.select(selectAuthData);
  }
  updateUser(updateData: UserUpdateDataRequest): Observable<boolean> {
    this.store.dispatch(updateUserData({ userUpdateData: updateData }));
    return this.store.select(selectUpdateIsSuccessful);
  }
  getAuthUserErrors(): Observable<any> {
    return this.store.select(selectAuthErrors);
  }
}