import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { jwtDecode, JwtPayload } from 'jwt-decode';
import { BehaviorSubject, filter, Observable, switchMap, take, throwError } from 'rxjs';
import { AuthenticationService } from '../..';
import { AuthData, AuthToken } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class AuthInterceptor implements HttpInterceptor {
  private authToken: AuthToken | null = null;
  private decodedToken: JwtPayload | null = null;
  private isRefreshing = false;
  private refreshTokenSubject: BehaviorSubject<any> = new BehaviorSubject<any>(null);

  constructor(
    private readonly authService: AuthenticationService
  ) {
    this.authService.getAuthData().subscribe(
      data => {
        this.processAuthData(data);
      }
    );
    this.authService.getAuthErrors().subscribe(errors => {
      if (errors !== null) {
        this.authToken = null;
        this.decodedToken = null;
        this.logOutUserWithError(errors);
      }
    });
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<Object>> {
    let authReq = req;
    if (authReq.url.includes('/refresh')) {
      return next.handle(authReq);
    }
    if (this.authToken != null) {
      authReq = this.addTokenHeader(req, this.authToken.accessToken);
    }
    if (this.isTokenExpired()) {
      return this.refreshToken(authReq, next);
    }

    return next.handle(authReq);
  }
  private refreshToken(request: HttpRequest<any>, next: HttpHandler) {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);
      if (this.authToken) {
        return this.authService.refreshToken(this.authToken).pipe(
          filter(isSuccess => isSuccess === true),
          take(1),
          switchMap(() => {
            this.isRefreshing = false;
            this.refreshTokenSubject.next(this.authToken!.accessToken);
            return next.handle(this.addTokenHeader(request, this.authToken!.accessToken));
          })
        );
      }
    }
    return this.refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap((token) => next.handle(this.addTokenHeader(request, token)))
    );
  }
  private addTokenHeader(req: HttpRequest<any>, token: string) {
    const clonedRequest = req.clone({
      headers: req.headers
        .set('Authorization', `Bearer ${token}`)
    });
    return clonedRequest;
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