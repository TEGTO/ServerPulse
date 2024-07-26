import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode, JwtPayload } from 'jwt-decode';
import { catchError, Observable, switchMap, throwError } from 'rxjs';
import { AuthenticationService } from '../..';
import { AuthData, AuthToken } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class AuthInterceptor implements HttpInterceptor {

  authToken!: AuthToken;
  decodedToken: JwtPayload | null = null;

  constructor(
    private readonly authService: AuthenticationService
  ) {
    authService.getAuthData().subscribe(
      data => {
        this.processAuthData(data);
      }
    )
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.headers.has('X-Skip-Interceptor') || !this.decodedToken) {
      return next.handle(req);
    }
    else {
      return this.interceptWithToken(req, next);
    }
  }
  private interceptWithToken(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (this.authToken.refreshTokenExpiryDate < new Date()) {
      return this.logOutUserWithError('Refresh token expired')
    }
    if (this.isTokenExpired()) {
      return this.authService.refreshToken(this.authToken).pipe(
        switchMap(data => {
          this.processAuthData(data);
          return this.sendRequestWithToken(req, next);
        }),
        catchError(error => {
          return this.logOutUserWithError(error.message)
        })
      );
    }
    else {
      return this.sendRequestWithToken(req, next);
    }
  }
  private sendRequestWithToken(req: HttpRequest<any>, next: HttpHandler) {
    const req1 = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${this.authToken.accessToken}`),
    });
    return next.handle(req1);
  }
  private logOutUserWithError(errorMessage: string) {
    this.authService.logOutUser();
    return throwError(() => new Error(errorMessage));
  }
  private isTokenExpired(): boolean {
    const decoded: any = this.decodedToken;
    if (decoded.exp) {
      const expirationDate = new Date(0);
      expirationDate.setUTCSeconds(decoded.exp);
      return expirationDate < new Date();
    }
    return false;
  }
  private tryDecodeToken(token: string) {
    return jwtDecode(token)
  }
  private processAuthData(data: AuthData) {
    this.authToken = {
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      refreshTokenExpiryDate: new Date(data.refreshTokenExpiryDate)
    };
    try {
      this.decodedToken = this.tryDecodeToken(this.authToken.accessToken);
    } catch (error) {
      this.decodedToken = null;
    }
  }
}