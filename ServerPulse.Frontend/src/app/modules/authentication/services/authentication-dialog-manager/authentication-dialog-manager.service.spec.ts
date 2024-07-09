import { TestBed } from '@angular/core/testing';

import { AuthenticationDialogManagerService } from './authentication-dialog-manager.service';

describe('AuthenticationDialogManagerService', () => {
  let service: AuthenticationDialogManagerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthenticationDialogManagerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
