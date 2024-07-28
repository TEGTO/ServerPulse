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

  private static isRefreshingToken = false;
  private readonly refreshTokenCheckInFutureInMinutes: number = 0;

  private authToken!: AuthToken;
  private decodedToken: JwtPayload | null = null;

  constructor(
    private readonly authService: AuthenticationService
  ) {
    this.authService.getAuthData().subscribe(
      data => {
        this.processAuthData(data);
      }
    );
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (req.headers.has('X-Skip-Interceptor') || !this.decodedToken) {
      return next.handle(req);
    } else {
      return this.interceptWithToken(req, next);
    }
  }
  private interceptWithToken(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (this.authToken.refreshTokenExpiryDate < new Date()) {
      return this.logOutUserWithError('Refresh token expired');
    }
    if (this.isTokenExpired() && !AuthInterceptor.isRefreshingToken) {
      AuthInterceptor.isRefreshingToken = true;
      return this.authService.refreshToken(this.authToken).pipe(
        switchMap(data => {
          this.processAuthData(data);
          AuthInterceptor.isRefreshingToken = false;
          console.log(data.refreshToken);
          return this.sendRequestWithToken(req, next);
        }),
        catchError(error => {
          AuthInterceptor.isRefreshingToken = false;
          return this.logOutUserWithError(error.message);
        })
      );
    } else {
      return this.sendRequestWithToken(req, next);
    }
  }
  private sendRequestWithToken(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const clonedRequest = req.clone({
      headers: req.headers
        .set('Authorization', `Bearer ${this.authToken.accessToken}`)
        .set('X-Skip-Interceptor', 'true'),
    });
    return next.handle(clonedRequest);
  }
  private logOutUserWithError(errorMessage: string): Observable<never> {
    this.authService.logOutUser();
    return throwError(() => new Error(errorMessage));
  }
  private isTokenExpired(): boolean {
    if (this.decodedToken?.exp) {
      const expirationDate = new Date(0);
      expirationDate.setUTCSeconds(this.decodedToken.exp);

      const currentDatePlusMinutes = new Date();
      currentDatePlusMinutes.setMinutes(currentDatePlusMinutes.getMinutes() + this.refreshTokenCheckInFutureInMinutes);

      return expirationDate < currentDatePlusMinutes;
    }
    return false;
  }
  private tryDecodeToken(token: string): JwtPayload | null {
    try {
      return jwtDecode<JwtPayload>(token);
    } catch (error) {
      return null;
    }
  }
  private processAuthData(data: AuthData): void {
    this.authToken = {
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      refreshTokenExpiryDate: new Date(data.refreshTokenExpiryDate)
    };
    this.decodedToken = this.tryDecodeToken(this.authToken.accessToken);
  }
}