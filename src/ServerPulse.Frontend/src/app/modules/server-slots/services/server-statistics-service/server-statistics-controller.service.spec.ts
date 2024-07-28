import { TestBed } from '@angular/core/testing';

import { ServerStatisticsControllerService } from './server-statistics-controller.service';

describe('ServerStatisticsControllerService', () => {
  let service: ServerStatisticsControllerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServerStatisticsControllerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
