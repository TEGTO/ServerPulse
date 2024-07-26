import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthToken, URLDefiner, UserAuthenticationRequest, UserAuthenticationResponse, UserRegistrationRequest, UserUpdateDataRequest } from '../../..';
import { AuthenticationApiService } from './authentication-api.service';

describe('AuthenticationApiService', () => {
  let httpTestingController: HttpTestingController;
  let service: AuthenticationApiService;
  let mockUrlDefiner: jasmine.SpyObj<URLDefiner>

  beforeEach(() => {
    mockUrlDefiner = jasmine.createSpyObj<URLDefiner>('URLDefiner', ['combineWithAuthApiUrl']);
    mockUrlDefiner.combineWithAuthApiUrl.and.callFake((subpath: string) => subpath);

    TestBed.configureTestingModule({
      providers: [
        { provide: URLDefiner, useValue: mockUrlDefiner }
      ],
      imports: [HttpClientTestingModule]
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
    const expectedReq = `/login`;
    let request: UserAuthenticationRequest =
    {
      login: "login",
      password: "password"
    }
    let response: UserAuthenticationResponse =
    {
      authToken: {
        accessToken: "",
        refreshToken: "",
        refreshTokenExpiryDate: new Date()
      },
      userName: "userName",
      email: "email"
    }

    service.loginUser(request).subscribe(x => {
      expect(x).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith(expectedReq);
    req.flush(response);
  });

  it('should register user', () => {
    const expectedReq = `/register`;
    let request: UserRegistrationRequest =
    {
      userName: "userName",
      email: "email",
      password: "password",
      confirmPassword: "confirmPassword"
    }

    service.registerUser(request).subscribe();

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith(expectedReq);
  });

  it('should refresh token', () => {
    const expectedReq = `/refresh`;
    let request: AuthToken =
    {
      accessToken: "",
      refreshToken: "",
      refreshTokenExpiryDate: new Date()
    };
    let response: AuthToken =
    {
      accessToken: "",
      refreshToken: "",
      refreshTokenExpiryDate: new Date()
    };

    service.refreshToken(request).subscribe(x => {
      expect(x).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith(expectedReq);
    req.flush(response);
  });

  it('should update user', () => {
    const expectedReq = `/update`;
    let request: UserUpdateDataRequest =
    {
      userName: "userName",
      oldEmail: "oldEmail",
      newEmail: "newEmail",
      oldPassword: "oldPassword",
      newPassword: "newPassword"
    };

    service.updateUser(request).subscribe();

    const req = httpTestingController.expectOne(expectedReq);
    expect(req.request.method).toBe('PUT');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith(expectedReq);
  });
});
