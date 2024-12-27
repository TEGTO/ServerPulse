import { HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { AuthData, GetOAuthUrl, GetOAuthUrlQueryParams, GetOAuthUrlResponse, mapGetOAuthUrlResponseToGetOAuthUrl, mapUserAuthenticationResponseToAuthData, UserAuthenticationResponse, UserOAuthenticationRequest } from '../..';
import { BaseApiService } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class OauthApiService extends BaseApiService {

  getOAuthUrl(req: GetOAuthUrlQueryParams): Observable<GetOAuthUrl> {
    const params = new HttpParams()
      .set('OAuthLoginProvider', req.oAuthLoginProvider)
      .set('redirectUrl', req.redirectUrl)
      .set('codeVerifier', req.codeVerifier);

    return this.httpClient.get<GetOAuthUrlResponse>(this.combinePathWithOAuthApiUrl(``), { params })
      .pipe(
        map((response) => mapGetOAuthUrlResponseToGetOAuthUrl(response)),
        catchError((resp) => this.handleError(resp))
      );
  }

  loginUserOAuth(req: UserOAuthenticationRequest): Observable<AuthData> {
    return this.httpClient.post<UserAuthenticationResponse>(this.combinePathWithOAuthApiUrl(``), req).pipe(
      map((response) => mapUserAuthenticationResponseToAuthData(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  private combinePathWithOAuthApiUrl(subpath: string): string {
    return this.urlDefiner.combineWithAuthApiUrl("/oauth" + subpath);
  }
}
