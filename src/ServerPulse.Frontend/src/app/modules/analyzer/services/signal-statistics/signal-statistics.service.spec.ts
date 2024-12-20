import { TestBed } from '@angular/core/testing';

import { SignalStatisticsService } from './signal-statistics.service';

describe('SignalStatisticsService', () => {
  let service: SignalStatisticsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SignalStatisticsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
