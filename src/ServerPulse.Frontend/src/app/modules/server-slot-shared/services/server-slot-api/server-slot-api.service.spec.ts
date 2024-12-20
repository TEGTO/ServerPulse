import { TestBed } from '@angular/core/testing';

import { ServerSlotApiService } from './server-slot-api.service';

describe('ServerSlotApiService', () => {
  let service: ServerSlotApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServerSlotApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
