import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import { Observable, switchMap, throwError } from 'rxjs';
import { AuthenticationService } from '../..';
import { AccessTokenDto } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class AuthInterceptor implements HttpInterceptor {

  tokenData!: AccessTokenDto;

  constructor(private authService: AuthenticationService) {
    authService.getAuthUserData().subscribe(
      data => {
        this.tokenData = {
          accessToken: data.authToken,
          refreshToken: data.refreshToken,
          refreshTokenExpiryDate: data.refreshTokenExpiryDate
        }
      }
    )
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.headers.has('X-Skip-Interceptor') || !this.tokenData.accessToken) {
      return next.handle(req);
    }
    if (this.tokenData.refreshTokenExpiryDate < new Date()) {
      this.authService.logOutUser();
      return throwError(() => new Error('Refresh token expired'));
    }
    if (this.isTokenExpired(this.tokenData.accessToken)) {
      return this.authService.refreshToken(this.tokenData).pipe(
        switchMap(data => {
          this.tokenData = {
            accessToken: data.authToken,
            refreshToken: data.refreshToken,
            refreshTokenExpiryDate: new Date(data.refreshTokenExpiryDate)
          };
          return this.sendRequest(req, next);
        })
      );
    }
    else {
      return this.sendRequest(req, next);
    }
  }
  private sendRequest(req: HttpRequest<any>, next: HttpHandler) {
    if (!this.tokenData.accessToken) {
      return next.handle(req);
    }
    const req1 = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${this.tokenData.accessToken}`),
    });
    return next.handle(req1);
  }
  private isTokenExpired(token: string): boolean {
    try {
      const decoded: any = jwtDecode(token);
      if (decoded.exp) {
        const expirationDate = new Date(0);
        expirationDate.setUTCSeconds(decoded.exp);
        return expirationDate < new Date();
      }
      return false;
    } catch (e) {
      return false;
    }
  }
}
