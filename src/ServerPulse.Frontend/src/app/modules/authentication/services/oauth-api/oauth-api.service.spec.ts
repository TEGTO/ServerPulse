import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthData, GetOAuthUrlParams, LoginOAuthRequest, OAuthLoginProvider } from '../..';
import { URLDefiner } from '../../../shared';
import { OauthApiService } from './oauth-api.service';

describe('OauthApiService', () => {
  let service: OauthApiService;
  let httpTestingController: HttpTestingController;
  let mockUrlDefiner: jasmine.SpyObj<URLDefiner>;

  beforeEach(() => {
    mockUrlDefiner = jasmine.createSpyObj<URLDefiner>('URLDefiner', ['combineWithAuthApiUrl']);
    mockUrlDefiner.combineWithAuthApiUrl.and.callFake((subpath: string) => `/api${subpath}`);

    TestBed.configureTestingModule({
      providers: [
        OauthApiService,
        { provide: URLDefiner, useValue: mockUrlDefiner },
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    service = TestBed.inject(OauthApiService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch OAuth URL', () => {
    const expectedUrl = `/api/oauth`;
    const requestParams: GetOAuthUrlParams = {
      oAuthLoginProvider: OAuthLoginProvider.Google,
      redirectUrl: 'https://redirect-url.com',
    };
    const response = { url: 'https://oauth-login-url.com' };

    service.getOAuthUrl(requestParams).subscribe((res) => {
      expect(res).toEqual({ url: response.url });
    });

    const req = httpTestingController.expectOne((req) =>
      req.url === expectedUrl &&
      req.params.get('OAuthLoginProvider') === OAuthLoginProvider.Google.toString() &&
      req.params.get('redirectUrl') === requestParams.redirectUrl
    );

    expect(req.request.method).toBe('GET');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith('/oauth');
    req.flush(response);
  });

  it('should log in the user via OAuth', () => {
    const expectedUrl = `/api/oauth`;
    const request: LoginOAuthRequest = {
      code: 'auth-code',
      redirectUrl: 'https://redirect-url.com',
      oAuthLoginProvider: OAuthLoginProvider.Google,
    };
    const response: AuthData = {
      isAuthenticated: true,
      accessTokenData: {
        accessToken: 'accessToken',
        refreshToken: 'refreshToken',
        refreshTokenExpiryDate: new Date(),
      },
      email: 'user@example.com',
    };

    service.loginUserOAuth(request).subscribe((res) => {
      expect(res).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedUrl);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith('/oauth');
    req.flush(response);
  });

  it('should handle error on fetch OAuth URL', () => {
    const expectedUrl = `/api/oauth?OAuthLoginProvider=0&redirectUrl=https://redirect-url.com`;
    const requestParams: GetOAuthUrlParams = {
      oAuthLoginProvider: OAuthLoginProvider.Google,
      redirectUrl: 'https://redirect-url.com',
    };

    service.getOAuthUrl(requestParams).subscribe({
      next: () => fail('Expected an error, not a success'),
      error: (error) => {
        expect(error).toBeTruthy();
      },
    });

    const req = httpTestingController.expectOne(expectedUrl);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });

  it('should handle error on login via OAuth', () => {
    const expectedUrl = `/api/oauth`;
    const request: LoginOAuthRequest = {
      code: 'auth-code',
      redirectUrl: 'https://redirect-url.com',
      oAuthLoginProvider: OAuthLoginProvider.Google,
    };

    service.loginUserOAuth(request).subscribe({
      next: () => fail('Expected an error, not a success'),
      error: (error) => {
        expect(error).toBeTruthy();
      },
    });

    const req = httpTestingController.expectOne(expectedUrl);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });
});
