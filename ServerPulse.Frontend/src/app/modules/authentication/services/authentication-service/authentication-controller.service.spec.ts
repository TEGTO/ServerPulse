import { TestBed } from '@angular/core/testing';

import { AuthenticationControllerService } from './authentication-controller.service';

describe('AuthenticationControllerService', () => {
  let service: AuthenticationControllerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthenticationControllerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
