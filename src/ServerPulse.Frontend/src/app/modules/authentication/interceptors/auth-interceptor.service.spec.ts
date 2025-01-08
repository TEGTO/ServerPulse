/* eslint-disable @typescript-eslint/no-explicit-any */
import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ErrorHandler } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Store } from '@ngrx/store';
import { BehaviorSubject, of } from 'rxjs';
import { AccessTokenData, AuthInterceptor, logOutUser, refreshAccessToken, selectAuthData, selectAuthErrors, selectIsRefreshSuccessful } from '..';

describe('AuthInterceptor', () => {
    const validAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjk5OTk5OTk5OTl9.fK3V4NMCKO2ozn8K18sRv9XUcDC2N2hvGTuXMuRcn5Y';
    const expiredAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjF9.5o2DmuhVcwInco2_YNzqMKLk-NGH44HoSQqX0CSzoaA';

    let httpMock: HttpTestingController;
    let httpClient: HttpClient;
    let interceptor: AuthInterceptor;
    let storeSpy: jasmine.SpyObj<Store>;

    const mockAuthToken: AccessTokenData = {
        accessToken: validAccessToken,
        refreshToken: 'valid-refresh-token',
        refreshTokenExpiryDate: new Date()
    };

    const expiredAuthToken: AccessTokenData = {
        accessToken: expiredAccessToken,
        refreshToken: 'valid-refresh-token',
        refreshTokenExpiryDate: new Date(0)
    };

    let authDataSubject: BehaviorSubject<any>;
    let authErrorSubject: BehaviorSubject<any>;

    beforeEach(() => {
        authDataSubject = new BehaviorSubject({
            accessTokenData: mockAuthToken,
            isAuthenticated: true,
        });
        authErrorSubject = new BehaviorSubject(null);

        storeSpy = jasmine.createSpyObj<Store>(['dispatch', 'select']);

        storeSpy.select.and.callFake((selector: any) => {
            if (selector === selectAuthData) {
                return authDataSubject.asObservable();
            } else if (selector === selectAuthErrors) {
                return authErrorSubject.asObservable();
            } else if (selector === selectIsRefreshSuccessful) {
                return of(true);
            } else {
                return of(null);
            }
        });

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                { provide: Store, useValue: storeSpy },
                { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
                { provide: ErrorHandler, useValue: jasmine.createSpyObj('ErrorHandler', ['handleError']) },
            ],
        });

        httpMock = TestBed.inject(HttpTestingController);
        httpClient = TestBed.inject(HttpClient);
        interceptor = TestBed.inject(AuthInterceptor);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should add an Authorization header', () => {
        httpClient.get('/test').subscribe(response => {
            expect(response).toBeTruthy();
        });

        const httpRequest = httpMock.expectOne('/test');
        expect(httpRequest.request.headers.has('Authorization')).toBe(true);
        expect(httpRequest.request.headers.get('Authorization')).toBe(`Bearer ${mockAuthToken.accessToken}`);

        httpRequest.flush({});
    });

    it('should skip interception for requests to /refresh', () => {
        httpClient.get('/refresh').subscribe(response => {
            expect(response).toBeTruthy();
        });

        const httpRequest = httpMock.expectOne('/refresh');
        expect(httpRequest.request.headers.has('Authorization')).toBe(false);

        httpRequest.flush({});
    });

    it('should logout user on auth error during refresh', () => {
        interceptor["isRefreshing"] = true;

        authErrorSubject.next('Invalid token error');

        httpClient.get('/test').subscribe({
            error: () => {
                expect(storeSpy.dispatch).toHaveBeenCalledWith(logOutUser());
            },
        });

        const httpRequest = httpMock.expectOne('/test');
        httpRequest.flush({}, { status: 401, statusText: 'Unauthorized' });
    });

    it('should refresh token if access token is expired', () => {
        authDataSubject.next({
            accessTokenData: expiredAuthToken,
            isAuthenticated: true,
        });

        httpClient.get('/test').subscribe();

        const httpRequest = httpMock.expectOne('/test');
        expect(storeSpy.dispatch).toHaveBeenCalledWith(refreshAccessToken({ accessTokenData: expiredAuthToken }));
        expect(httpRequest.request.headers.has('Authorization')).toBe(true);

        httpRequest.flush({});
    });

    it('should wait for token refresh if already in progress', () => {
        httpClient.get('/test').subscribe();

        const httpRequest = httpMock.expectOne('/test');
        expect(httpRequest.request.headers.get('Authorization')).toBe(`Bearer ${mockAuthToken.accessToken}`);

        httpRequest.flush({});
    });
});