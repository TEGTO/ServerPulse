import { TestBed } from '@angular/core/testing';

import { HttpClientTestingModule } from '@angular/common/http/testing';
import { StatisticsApiService } from './statistics-api.service';

describe('StatisticsApiService', () => {
  let service: StatisticsApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [

      ],
      imports: [HttpClientTestingModule]
    });
    service = TestBed.inject(StatisticsApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
