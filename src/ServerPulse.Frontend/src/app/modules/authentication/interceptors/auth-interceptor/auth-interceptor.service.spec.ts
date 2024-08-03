import { HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { BehaviorSubject, of } from 'rxjs';
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
  let authDataSubject: BehaviorSubject<AuthData>;

  beforeEach(() => {
    authDataSubject = new BehaviorSubject<AuthData>(mockAuthData);
    authService = jasmine.createSpyObj('AuthenticationService', ['getAuthData', 'getAuthErrors', 'refreshToken', 'logOutUser']);
    authService.getAuthData.and.returnValue(authDataSubject.asObservable());
    authService.getAuthErrors.and.returnValue(of(null));

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
        { provide: AuthenticationService, useValue: authService },
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    httpClient = TestBed.inject(HttpClient);
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

  it('should skip interception for requests with /refresh in URL', (done) => {
    httpClient.get('/refresh').subscribe(response => {
      expect(response).toBeTruthy();
      done();
    });

    const httpRequest = httpMock.expectOne('/refresh');

    expect(httpRequest.request.headers.has('Authorization')).toBe(false);

    httpRequest.flush({});
  });

  it('should logout if auth error', (done) => {

    authService.getAuthErrors.and.returnValue(of("Error"));

    httpClient.get('/test').subscribe(response => {
      expect(response).toBeTruthy();
      done();
    });
    expect(authService.logOutUser).toHaveBeenCalled();
    const httpRequest = httpMock.expectOne('/test');
    httpRequest.flush({});
  });

  it('should refresh token if access token is expired', (done) => {
    const newAuthData: AuthData = {
      ...mockAuthData,
      accessToken: 'new-access-token'
    };

    authService.getAuthData.and.returnValue(of({ ...mockAuthData, accessToken: expiredAccessToken }));
    authService.refreshToken.and.returnValue(of(true));

    httpClient.get('/test').subscribe(response => {
      expect(response).toBeTruthy();
      done();
    });

    const httpRequest = httpMock.expectOne('/test');

    expect(authService.refreshToken).toHaveBeenCalled();

    httpRequest.flush({});
  });
});