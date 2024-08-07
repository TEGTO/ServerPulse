import { TestBed } from '@angular/core/testing';

import { StatisticsCollector } from './statistics-collector.service';

describe('ServerStatisticsControllerService', () => {
  let service: StatisticsCollector;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(StatisticsCollector);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
