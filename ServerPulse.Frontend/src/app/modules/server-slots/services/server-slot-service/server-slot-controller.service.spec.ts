import { TestBed } from '@angular/core/testing';

import { ServerSlotControllerService } from './server-slot-controller.service';

describe('ServerSlotControllerService', () => {
  let service: ServerSlotControllerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServerSlotControllerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
