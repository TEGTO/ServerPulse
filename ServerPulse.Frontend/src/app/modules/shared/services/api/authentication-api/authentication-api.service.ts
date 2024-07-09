import { Injectable } from '@angular/core';
import { Observable, catchError } from 'rxjs';
import { AccessTokenDto, UserAuthenticationRequest, UserRegistrationRequest, UserUpdateDataRequest } from '../../..';
import { BaseApiService } from '../base-api/base-api.service';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationApiService extends BaseApiService {

  loginUser(userAuthData: UserAuthenticationRequest): Observable<AccessTokenDto> {
    return this.httpClient.post<AccessTokenDto>(this.combinePathWithAuthApiUrl(`/login`), userAuthData).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  registerUser(userRegistrationData: UserRegistrationRequest) {
    return this.httpClient.post(this.combinePathWithAuthApiUrl(`/register`), userRegistrationData).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  refreshToken(tokenData: AccessTokenDto): Observable<AccessTokenDto> {
    const headers = { 'X-Skip-Interceptor': 'true' };
    return this.httpClient.post<AccessTokenDto>(this.combinePathWithAuthApiUrl(`/refresh`), tokenData, { headers }).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  updateUser(updateUserData: UserUpdateDataRequest) {
    return this.httpClient.put(this.combinePathWithAuthApiUrl(`/update`), updateUserData).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  private combinePathWithAuthApiUrl(subpath: string) {
    return this.urlDefiner.combineWithAuthApiUrl(subpath);
  }
}