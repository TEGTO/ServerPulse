import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthData, AuthToken, UserAuthenticationRequest, UserRegistrationRequest } from '../..';
import { URLDefiner } from '../../../shared';
import { AuthenticationApiService } from './authentication-api.service';

describe('AuthenticationApiService', () => {
  let service: AuthenticationApiService;
  let httpTestingController: HttpTestingController;
  let mockUrlDefiner: jasmine.SpyObj<URLDefiner>;

  beforeEach(() => {
    mockUrlDefiner = jasmine.createSpyObj<URLDefiner>('URLDefiner', ['combineWithAuthApiUrl']);
    mockUrlDefiner.combineWithAuthApiUrl.and.callFake((subpath: string) => `/api${subpath}`);

    TestBed.configureTestingModule({
      providers: [
        AuthenticationApiService,
        { provide: URLDefiner, useValue: mockUrlDefiner },
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(AuthenticationApiService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should login user', () => {
    const expectedReq = `/api/auth/login`;
    const request: UserAuthenticationRequest = {
      login: 'login',
      password: 'password'
    };
    const response: AuthData = {
      isAuthenticated: true,
      authToken: {
        accessToken: 'accessToken',
        refreshToken: 'refreshToken',
        refreshTokenExpiryDate: new Date()
      },
      email: 'userName',
    };

    service.loginUser(request).subscribe(res => {
      expect(res).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith('/auth/login');
    req.flush(response);
  });

  it('should register user', () => {
    const expectedReq = `/api/auth/register`;
    const request: UserRegistrationRequest = {
      email: 'user@example.com',
      password: 'password',
      confirmPassword: 'password'
    };
    const response: AuthData = {
      isAuthenticated: true,
      authToken: {
        accessToken: 'accessToken',
        refreshToken: 'refreshToken',
        refreshTokenExpiryDate: new Date()
      },
      email: 'user@example.com',
    };

    service.registerUser(request).subscribe(res => {
      expect(res).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith('/auth/register');
    req.flush(response);
  });

  it('should refresh token', () => {
    const expectedReq = `/api/auth/refresh`;
    const request: AuthToken = {
      accessToken: 'oldAccessToken',
      refreshToken: 'oldRefreshToken',
      refreshTokenExpiryDate: new Date()
    };
    const response: AuthToken = {
      accessToken: 'newAccessToken',
      refreshToken: 'newRefreshToken',
      refreshTokenExpiryDate: new Date()
    };

    service.refreshToken(request).subscribe(res => {
      expect(res).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith('/auth/refresh');
    req.flush(response);
  });

  it('should handle error on register user', () => {
    const expectedReq = `/api/auth/register`;
    const request: UserRegistrationRequest = {
      email: 'user@example.com',
      password: 'password',
      confirmPassword: 'password'
    };

    service.registerUser(request).subscribe({
      next: () => fail('Expected an error, not a success'),
      error: (error) => {
        expect(error).toBeTruthy();
      }
    });

    const req = httpTestingController.expectOne(expectedReq);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });

  it('should handle error on refresh token', () => {
    const expectedReq = `/api/auth/refresh`;
    const request: AuthToken = {
      accessToken: 'oldAccessToken',
      refreshToken: 'oldRefreshToken',
      refreshTokenExpiryDate: new Date()
    };

    service.refreshToken(request).subscribe({
      next: () => fail('Expected an error, not a success'),
      error: (error) => {
        expect(error).toBeTruthy();
      }
    });


    const req = httpTestingController.expectOne(expectedReq);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });
});
