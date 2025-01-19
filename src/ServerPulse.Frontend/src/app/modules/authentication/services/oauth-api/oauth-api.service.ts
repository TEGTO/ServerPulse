import { HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { AuthData, GetOAuthUrl, GetOAuthUrlParams, GetOAuthUrlResponse, LoginOAuthRequest, LoginOAuthResponse, mapGetOAuthUrlResponseToGetOAuthUrl, mapUserAuthenticationResponseToAuthData } from '../..';
import { BaseApiService } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class OauthApiService extends BaseApiService {

  getOAuthUrl(req: GetOAuthUrlParams): Observable<GetOAuthUrl> {
    const params = new HttpParams()
      .set('OAuthLoginProvider', req.oAuthLoginProvider)
      .set('redirectUrl', req.redirectUrl);

    return this.httpClient.get<GetOAuthUrlResponse>(this.combinePathWithOAuthApiUrl(``), { params })
      .pipe(
        map((response) => mapGetOAuthUrlResponseToGetOAuthUrl(response)),
        catchError((resp) => this.handleError(resp))
      );
  }

  loginUserOAuth(req: LoginOAuthRequest): Observable<AuthData> {
    return this.httpClient.post<LoginOAuthResponse>(this.combinePathWithOAuthApiUrl(``), req).pipe(
      map((response) => mapUserAuthenticationResponseToAuthData(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  private combinePathWithOAuthApiUrl(subpath: string): string {
    return this.urlDefiner.combineWithAuthApiUrl("/oauth" + subpath);
  }
}
