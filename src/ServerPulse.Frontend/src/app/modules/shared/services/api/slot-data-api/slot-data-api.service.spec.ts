import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ErrorHandler, SlotDataResponse, URLDefiner } from '../../..';
import { SlotDataApiService } from './slot-data-api.service';

describe('SlotDataApiService', () => {
  let service: SlotDataApiService;
  let httpMock: HttpTestingController;
  let mockUrlDefiner: jasmine.SpyObj<URLDefiner>;
  let mockErrorHandler: jasmine.SpyObj<ErrorHandler>;

  beforeEach(() => {
    mockUrlDefiner = jasmine.createSpyObj('URLDefiner', ['combineWithSlotDataApiUrl']);
    mockErrorHandler = jasmine.createSpyObj('ErrorHandler', ['handleApiError']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        SlotDataApiService,
        { provide: URLDefiner, useValue: mockUrlDefiner },
        { provide: ErrorHandler, useValue: mockErrorHandler },
      ]
    });

    service = TestBed.inject(SlotDataApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getSlotData should return expected data (HttpClient called once)', () => {
    const key = 'test-key';
    const expectedUrl = `http://api.example.com/slotdata/${key}`;
    const expectedResponse: SlotDataResponse = {
      collectedDateUTC: new Date(),
      generalStatistics: null,
      loadStatistics: null,
      customEventStatistics: null,
      lastLoadEvents: [],
      lastCustomEvents: []
    };

    mockUrlDefiner.combineWithSlotDataApiUrl.and.returnValue(expectedUrl);

    service.getSlotData(key).subscribe(data => {
      expect(data).toEqual(expectedResponse);
    });

    const req = httpMock.expectOne(expectedUrl);
    expect(req.request.method).toBe('GET');
    req.flush(expectedResponse);
  });

  it('getSlotData should handle error response', () => {
    const key = 'test-key';
    const expectedUrl = `http://api.example.com/slotdata/${key}`;
    const errorResponse = { status: 404, statusText: 'Not Found' };

    mockUrlDefiner.combineWithSlotDataApiUrl.and.returnValue(expectedUrl);
    mockErrorHandler.handleApiError.and.returnValue('Error Occurred');

    service.getSlotData(key).subscribe({
      next: () => fail('expected an error, not data'),
      error: error => expect(error.message).toContain('Error Occurred')
    });

    const req = httpMock.expectOne(expectedUrl);
    req.flush('Error Occurred', errorResponse);
  });
});