import { TestBed } from '@angular/core/testing';

import { ServerSlotInfoDialogManagerService } from './server-slot-info-dialog-manager.service';

describe('ServerSlotInfoDialogManagerService', () => {
  let service: ServerSlotInfoDialogManagerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServerSlotInfoDialogManagerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
