import { TestBed } from '@angular/core/testing';

import { AnalyzerApiService } from './analyzer-api.service';

describe('AnalyzerApiService', () => {
  let service: AnalyzerApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AnalyzerApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
