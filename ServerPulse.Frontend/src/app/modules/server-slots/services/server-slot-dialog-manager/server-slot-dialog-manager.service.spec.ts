import { TestBed } from '@angular/core/testing';

import { ServerSlotDialogManagerService } from './server-slot-dialog-manager.service';

describe('ServerSlotDialogManagerService', () => {
  let service: ServerSlotDialogManagerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServerSlotDialogManagerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
