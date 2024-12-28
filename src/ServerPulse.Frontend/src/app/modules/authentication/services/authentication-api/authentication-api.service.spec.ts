import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthData, AuthToken, EmailConfirmationRequest, UserAuthenticationRequest, UserRegistrationRequest, UserUpdateRequest } from '../..';
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
    const request: UserAuthenticationRequest = { login: 'testuser', password: 'password123' };
    const response: AuthData = {
      isAuthenticated: true,
      authToken: {
        accessToken: 'accessToken123',
        refreshToken: 'refreshToken123',
        refreshTokenExpiryDate: new Date()
      },
      email: 'testuser@example.com'
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
      redirectConfirmUrl: 'http://example.com/confirm',
      email: 'testuser@example.com',
      password: 'password123',
      confirmPassword: 'password123'
    };

    service.registerUser(request).subscribe(res => {
      expect(res.status).toBe(200);
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith('/auth/register');
    req.flush({}, { status: 200, statusText: 'OK' });
  });

  it('should confirm email', () => {
    const expectedReq = `/api/auth/confirmation`;
    const request: EmailConfirmationRequest = { email: 'testuser@example.com', token: 'confirmationToken' };
    const response: AuthData = {
      isAuthenticated: true,
      authToken: {
        accessToken: 'accessToken123',
        refreshToken: 'refreshToken123',
        refreshTokenExpiryDate: new Date()
      },
      email: 'testuser@example.com'
    };

    service.confirmEmail(request).subscribe(res => {
      expect(res).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith('/auth/confirmation');
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

  it('should update user', () => {
    const expectedReq = `/api/auth/update`;
    const request: UserUpdateRequest = {
      email: 'updated@example.com',
      oldPassword: 'oldPassword123',
      password: 'newPassword123'
    };

    service.updateUser(request).subscribe(res => {
      expect(res.status).toBe(200);
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('PUT');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith('/auth/update');
    req.flush({}, { status: 200, statusText: 'OK' });
  });

  it('should handle errors on login', () => {
    const expectedReq = `/api/auth/login`;
    const request: UserAuthenticationRequest = { login: 'testuser', password: 'password123' };

    service.loginUser(request).subscribe({
      next: () => fail('Expected an error'),
      error: error => {
        expect(error).toBeTruthy();
      }
    });

    const req = httpTestingController.expectOne(expectedReq);
    req.flush('Error', { status: 401, statusText: 'Unauthorized' });
  });

  it('should handle errors on confirmation email', () => {
    const expectedReq = `/api/auth/confirmation`;
    const request: EmailConfirmationRequest = { email: 'testuser@example.com', token: 'invalidToken' };

    service.confirmEmail(request).subscribe({
      next: () => fail('Expected an error'),
      error: error => {
        expect(error).toBeTruthy();
      }
    });

    const req = httpTestingController.expectOne(expectedReq);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });
});
