/* eslint-disable @typescript-eslint/no-explicit-any */
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable, OnDestroy } from '@angular/core';
import { Store } from '@ngrx/store';
import { jwtDecode, JwtPayload } from 'jwt-decode';
import { BehaviorSubject, filter, Observable, Subscription, switchMap, take, throwError } from 'rxjs';
import { AccessTokenData, AuthData, logOutUser, refreshAccessToken, selectAuthData, selectAuthErrors, selectIsRefreshSuccessful } from '..';
import { ErrorHandler } from '../../shared';
@Injectable({
  providedIn: 'root'
})
export class AuthInterceptor implements HttpInterceptor, OnDestroy {
  private accessTokenData: AccessTokenData | null = null;
  private decodedToken: JwtPayload | null = null;
  private isRefreshing = false;
  private isAuthenticated = false;
  private readonly refreshTokenSubject$: BehaviorSubject<any> = new BehaviorSubject<any>(null);
  private readonly subscriptions: Subscription = new Subscription();

  constructor(
    private readonly store: Store,
    private readonly errorHandler: ErrorHandler
  ) {
    this.subscriptions.add(
      this.store.select(selectAuthData).subscribe(data => {
        this.processAuthData(data);
      })
    );

    this.subscriptions.add(
      this.store.select(selectAuthErrors).subscribe(errors => {
        if (errors !== null && this.isRefreshing) {
          this.isRefreshing = false;
          this.accessTokenData = null;
          this.decodedToken = null;
          this.logOutUserWithError(errors);
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<object>> {
    let authReq = req;

    if (authReq.url.includes('/refresh') || !this.isAuthenticated) {
      return next.handle(authReq);
    }

    if (this.accessTokenData != null) {
      authReq = this.addTokenHeader(req, this.accessTokenData.accessToken);
    }

    if (this.isTokenExpired()) {
      return this.refreshToken(authReq, next);
    }

    return next.handle(authReq);
  }

  private refreshToken(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;

      this.refreshTokenSubject$.next(null);

      if (this.accessTokenData) {
        this.store.dispatch(refreshAccessToken({ accessTokenData: this.accessTokenData }));

        return this.store.select(selectIsRefreshSuccessful).pipe(
          filter(isSuccess => isSuccess === true),
          take(1),
          switchMap(() => {
            this.isRefreshing = false;
            this.refreshTokenSubject$.next(this.accessTokenData!.accessToken);
            return next.handle(this.addTokenHeader(request, this.accessTokenData!.accessToken));
          })
        );
      }
    }
    return this.refreshTokenSubject$.pipe(
      filter(token => token !== null),
      take(1),
      switchMap((token) => next.handle(this.addTokenHeader(request, token)))
    );
  }

  private addTokenHeader(req: HttpRequest<any>, token: string): HttpRequest<any> {
    const clonedRequest = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
    return clonedRequest;
  }

  private logOutUserWithError(errorMessage: string): Observable<never> {
    this.store.dispatch(logOutUser());
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
      this.errorHandler.handleError(error);
      return null;
    }
  }

  private processAuthData(data: AuthData): void {
    this.accessTokenData = data.accessTokenData;
    this.isAuthenticated = data.isAuthenticated;

    if (this.isAuthenticated) {
      this.decodedToken = this.tryDecodeToken(this.accessTokenData.accessToken);
    }
  }
}