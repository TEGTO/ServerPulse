import { HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { AccessTokenData, AuthData, ConfirmEmailRequest, ConfirmEmailResponse, LoginRequest, LoginResponse, mapConfirmEmailResponseToAuthData, mapLoginResponseToAuthData, mapRefreshTokenResponseToAuthToken, RefreshTokenRequest, RefreshTokenResponse, RegisterRequest, UserUpdateRequest } from '../..';
import { BaseApiService } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationApiService extends BaseApiService {

  loginUser(req: LoginRequest): Observable<AuthData> {
    return this.httpClient.post<LoginResponse>(this.combinePathWithAuthApiUrl(`/login`), req).pipe(
      map((response) => mapLoginResponseToAuthData(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  registerUser(req: RegisterRequest): Observable<HttpResponse<void>> {
    return this.httpClient.post<void>(this.combinePathWithAuthApiUrl(`/register`), req, { observe: 'response' }).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }

  confirmEmail(req: ConfirmEmailRequest): Observable<AuthData> {
    return this.httpClient.post<ConfirmEmailResponse>(this.combinePathWithAuthApiUrl(`/confirmation`), req).pipe(
      map((response) => mapConfirmEmailResponseToAuthData(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  refreshToken(req: RefreshTokenRequest): Observable<AccessTokenData> {
    return this.httpClient.post<RefreshTokenResponse>(this.combinePathWithAuthApiUrl(`/refresh`), req).pipe(
      map((response) => mapRefreshTokenResponseToAuthToken(response)),
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
