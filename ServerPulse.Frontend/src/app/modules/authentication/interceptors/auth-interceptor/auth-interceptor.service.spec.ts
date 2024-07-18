import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { AuthenticationService } from '../..';
import { AuthData, AuthToken } from '../../../shared';
import { AuthInterceptor } from './auth-interceptor.service';

describe('AuthInterceptor', () => {
  const validAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjk5OTk5OTk5OTl9.fK3V4NMCKO2ozn8K18sRv9XUcDC2N2hvGTuXMuRcn5Y';
  const expiredAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjF9.5o2DmuhVcwInco2_YNzqMKLk-NGH44HoSQqX0CSzoaA';

  let httpMock: HttpTestingController;
  let httpClient: HttpClient;
  let authService: jasmine.SpyObj<AuthenticationService>;

  const mockAuthToken: AuthToken = {
    accessToken: validAccessToken,
    refreshToken: 'refresh-token',
    refreshTokenExpiryDate: new Date(new Date().getTime() + 60000) // 1 minute in the future
  };

  const mockAuthData: AuthData = {
    isAuthenticated: true,
    accessToken: validAccessToken,
    refreshToken: 'refresh-token',
    refreshTokenExpiryDate: new Date(new Date().getTime() + 60000) // 1 minute in the future
  };

  beforeEach(() => {
    const authServiceSpy = jasmine.createSpyObj('AuthenticationService', ['getAuthData', 'refreshToken', 'logOutUser']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
        { provide: AuthenticationService, useValue: authServiceSpy }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    httpClient = TestBed.inject(HttpClient);
    authService = TestBed.inject(AuthenticationService) as jasmine.SpyObj<AuthenticationService>;

    authService.getAuthData.and.returnValue(of(mockAuthData));
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should add an Authorization header', (done) => {
    httpClient.get('/test').subscribe(response => {
      expect(response).toBeTruthy();
      done();
    });

    const httpRequest = httpMock.expectOne('/test');

    expect(httpRequest.request.headers.has('Authorization')).toEqual(true);
    expect(httpRequest.request.headers.get('Authorization')).toBe(`Bearer ${mockAuthToken.accessToken}`);

    httpRequest.flush({});
  });

  it('should skip interception for requests with X-Skip-Interceptor header', (done) => {
    httpClient.get('/test', { headers: { 'X-Skip-Interceptor': 'true' } }).subscribe(response => {
      expect(response).toBeTruthy();
      done();
    });

    const httpRequest = httpMock.expectOne('/test');

    expect(httpRequest.request.headers.has('Authorization')).toEqual(false);

    httpRequest.flush({});
  });

  it('should logout if refresh token is expired', (done) => {
    const expiredTokenData: AuthToken = { ...mockAuthToken, refreshTokenExpiryDate: new Date(new Date().getTime() - 60000) }; // 1 minute in the past
    authService.getAuthData.and.returnValue(of({ ...mockAuthData, refreshTokenExpiryDate: expiredTokenData.refreshTokenExpiryDate }));

    httpClient.get('/test').subscribe({
      error: (error) => {
        expect(authService.logOutUser).toHaveBeenCalled();
        expect(error.message).toBe('Refresh token expired');
        done();
      }
    });

    httpMock.expectNone('/test');
  });

  it('should refresh token if access token is expired', (done) => {
    const newAuthData: AuthData = {
      ...mockAuthData,
    };

    authService.getAuthData.and.returnValue(of({ ...mockAuthData, accessToken: expiredAccessToken }));
    authService.refreshToken.and.returnValue(of(newAuthData));

    httpClient.get('/test').subscribe(response => {
      expect(response).toBeTruthy();
      done();
    });

    const httpRequest = httpMock.expectOne('/test');

    expect(authService.refreshToken).toHaveBeenCalled();
    expect(httpRequest.request.headers.has('Authorization')).toEqual(true);
    expect(httpRequest.request.headers.get('Authorization')).toBe(`Bearer ${newAuthData.accessToken}`);

    httpRequest.flush({});
  });

  it('should handle error if token refresh fails', (done) => {
    authService.getAuthData.and.returnValue(of({ ...mockAuthData, accessToken: expiredAccessToken }));
    authService.refreshToken.and.returnValue(throwError(() => new Error('Token refresh failed')));

    httpClient.get('/test').subscribe({
      error: (error) => {
        expect(authService.logOutUser).toHaveBeenCalled();
        expect(error.message).toBe('Token refresh failed');
        done();
      }
    });

    httpMock.expectNone('/test');
  });
});