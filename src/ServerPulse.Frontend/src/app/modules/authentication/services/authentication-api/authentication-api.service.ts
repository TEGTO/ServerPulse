import { HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { AuthData, AuthToken, AuthTokenResponse, EmailConfirmationRequest, mapAuthTokenResponseToAuthToken, mapUserAuthenticationResponseToAuthData, UserAuthenticationRequest, UserAuthenticationResponse, UserRegistrationRequest, UserUpdateRequest } from '../..';
import { BaseApiService } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationApiService extends BaseApiService {

  loginUser(req: UserAuthenticationRequest): Observable<AuthData> {
    return this.httpClient.post<UserAuthenticationResponse>(this.combinePathWithAuthApiUrl(`/login`), req).pipe(
      map((response) => mapUserAuthenticationResponseToAuthData(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  registerUser(req: UserRegistrationRequest): Observable<HttpResponse<void>> {
    return this.httpClient.post<void>(this.combinePathWithAuthApiUrl(`/register`), req, { observe: 'response' }).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }

  confirmEmail(req: EmailConfirmationRequest): Observable<AuthData> {
    return this.httpClient.post<UserAuthenticationResponse>(this.combinePathWithAuthApiUrl(`/confirmation`), req).pipe(
      map((response) => mapUserAuthenticationResponseToAuthData(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  refreshToken(tokenData: AuthToken): Observable<AuthToken> {
    return this.httpClient.post<AuthTokenResponse>(this.combinePathWithAuthApiUrl(`/refresh`), tokenData).pipe(
      map((response) => mapAuthTokenResponseToAuthToken(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  updateUser(req: UserUpdateRequest): Observable<HttpResponse<void>> {
    return this.httpClient.put<void>(this.combinePathWithAuthApiUrl(`/update`), req, { observe: 'response' }).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }

  private combinePathWithAuthApiUrl(subpath: string): string {
    return this.urlDefiner.combineWithAuthApiUrl("/auth" + subpath);
  }
}
