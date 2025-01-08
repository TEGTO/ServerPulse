import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { CustomEventResponse, GetLoadAmountStatisticsInRangeRequest, GetLoadEventsInDataRangeRequest, GetSlotStatisticsResponse, GetSomeCustomEventsRequest, GetSomeLoadEventsRequest, LoadAmountStatisticsResponse, LoadEventResponse, mapCustomEventResponseToCustomEvent, mapGetSlotStatisticsResponseToSlotStatistics, mapLoadAmountStatisticsResponseToLoadAmountStatistics, mapLoadEventResponseToLoadEvent } from '../..';
import { ErrorHandler, URLDefiner } from '../../../shared';
import { AnalyzerApiService } from './analyzer-api.service';

describe('AnalyzerApiService', () => {
  let service: AnalyzerApiService;
  let httpTestingController: HttpTestingController;
  let mockUrlDefiner: jasmine.SpyObj<URLDefiner>;
  let mockErrorHandler: jasmine.SpyObj<ErrorHandler>;

  beforeEach(() => {
    mockUrlDefiner = jasmine.createSpyObj<URLDefiner>('URLDefiner', ['combinePathWithAnalyzerApiUrl']);
    mockErrorHandler = jasmine.createSpyObj<ErrorHandler>('ErrorHandler', ['handleApiError']);

    mockUrlDefiner.combinePathWithAnalyzerApiUrl.and.callFake((subpath: string) => `/api${subpath}`);

    TestBed.configureTestingModule({
      providers: [
        AnalyzerApiService,
        { provide: URLDefiner, useValue: mockUrlDefiner },
        { provide: ErrorHandler, useValue: mockErrorHandler },
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(AnalyzerApiService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch load events in data range', () => {
    const request: GetLoadEventsInDataRangeRequest = {
      key: 'slotKey',
      from: new Date('2023-01-01T00:00:00Z'),
      to: new Date('2023-01-02T00:00:00Z'),
    };

    const response: LoadEventResponse[] = [
      {
        id: '1',
        key: 'slotKey',
        creationDateUTC: new Date('2023-01-01T00:00:00Z'),
        endpoint: '/test',
        method: 'GET',
        statusCode: 200,
        duration: '00:00:01',
        timestampUTC: new Date('2023-01-01T00:00:00Z'),
      }
    ];

    const expectedResult = response.map(mapLoadEventResponseToLoadEvent);

    service.getLoadEventsInDataRange(request).subscribe(res => {
      expect(res).toEqual(expectedResult);
    });

    const req = httpTestingController.expectOne('/api/analyze/daterange');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    expect(mockUrlDefiner.combinePathWithAnalyzerApiUrl).toHaveBeenCalledWith('/analyze/daterange');
    req.flush(response);
  });

  it('should handle error on getLoadEventsInDataRange', () => {
    const expectedReq = `/api/analyze/daterange`;
    const request: GetLoadEventsInDataRangeRequest = {
      key: 'slotKey',
      from: new Date('2023-01-01T00:00:00Z'),
      to: new Date('2023-01-02T00:00:00Z'),
    };

    service.getLoadEventsInDataRange(request).subscribe({
      next: () => fail('Expected an error, not a success'),
      error: (error) => {
        expect(error).toBeTruthy();
        expect(mockErrorHandler.handleApiError).toHaveBeenCalled();
      }
    });

    const req = httpTestingController.expectOne(expectedReq);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });

  it('should fetch daily load statistics', () => {
    const key = 'slotKey';
    const response: LoadAmountStatisticsResponse[] = [
      {
        id: '1',
        collectedDateUTC: new Date('2023-01-01T00:00:00Z'),
        amountOfEvents: 10,
        dateFrom: new Date('2023-01-01T00:00:00Z'),
        dateTo: new Date('2023-01-02T00:00:00Z'),
      }
    ];

    const expectedResult = response.map(mapLoadAmountStatisticsResponseToLoadAmountStatistics);

    service.getDailyLoadStatistics(key).subscribe(res => {
      expect(res).toEqual(expectedResult);
    });

    const req = httpTestingController.expectOne(`/api/analyze/perday/${key}`);
    expect(req.request.method).toBe('GET');
    expect(mockUrlDefiner.combinePathWithAnalyzerApiUrl).toHaveBeenCalledWith(`/analyze/perday/${key}`);
    req.flush(response);
  });

  it('should handle error on getDailyLoadStatistics', () => {
    const key = 'slotKey';
    const expectedReq = `/api/analyze/perday/${key}`;

    service.getDailyLoadStatistics(key).subscribe({
      next: () => fail('Expected an error, not a success'),
      error: (error) => {
        expect(error).toBeTruthy();
        expect(mockErrorHandler.handleApiError).toHaveBeenCalled();
      }
    });

    const req = httpTestingController.expectOne(expectedReq);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });

  it('should fetch load amount statistics in range', () => {
    const request: GetLoadAmountStatisticsInRangeRequest = {
      key: 'slotKey',
      from: new Date('2023-01-01T00:00:00Z'),
      to: new Date('2023-01-02T00:00:00Z'),
      timeSpan: '00:15:00'
    };

    const response: LoadAmountStatisticsResponse[] = [
      {
        id: '1',
        collectedDateUTC: new Date('2023-01-01T00:00:00Z'),
        amountOfEvents: 15,
        dateFrom: new Date('2023-01-01T00:00:00Z'),
        dateTo: new Date('2023-01-02T00:00:00Z'),
      }
    ];

    const expectedResult = response.map(mapLoadAmountStatisticsResponseToLoadAmountStatistics);

    service.getLoadAmountStatisticsInRange(request).subscribe(res => {
      expect(res).toEqual(expectedResult);
    });

    const req = httpTestingController.expectOne('/api/analyze/amountrange');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    expect(mockUrlDefiner.combinePathWithAnalyzerApiUrl).toHaveBeenCalledWith('/analyze/amountrange');
    req.flush(response);
  });

  it('should handle error on getLoadAmountStatisticsInRange', () => {
    const expectedReq = `/api/analyze/amountrange`;
    const request: GetLoadAmountStatisticsInRangeRequest = {
      key: 'slotKey',
      from: new Date('2023-01-01T00:00:00Z'),
      to: new Date('2023-01-02T00:00:00Z'),
      timeSpan: '00:15:00'
    };

    service.getLoadAmountStatisticsInRange(request).subscribe({
      next: () => fail('Expected an error, not a success'),
      error: (error) => {
        expect(error).toBeTruthy();
        expect(mockErrorHandler.handleApiError).toHaveBeenCalled();
      }
    });

    const req = httpTestingController.expectOne(expectedReq);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });

  it('should fetch some load events', () => {
    const request: GetSomeCustomEventsRequest = {
      key: 'slotKey',
      numberOfMessages: 5,
      startDate: new Date('2023-01-01T00:00:00Z'),
      readNew: true
    };

    const response: LoadEventResponse[] = [
      {
        id: '1',
        key: 'slotKey',
        creationDateUTC: new Date('2023-01-01T00:00:00Z'),
        endpoint: '/test',
        method: 'POST',
        statusCode: 201,
        duration: '00:00:02',
        timestampUTC: new Date('2023-01-01T00:00:00Z'),
      }
    ];

    const expectedResult = response.map(mapLoadEventResponseToLoadEvent);

    service.getSomeLoadEvents(request).subscribe(res => {
      expect(res).toEqual(expectedResult);
    });

    const req = httpTestingController.expectOne('/api/analyze/someevents');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    expect(mockUrlDefiner.combinePathWithAnalyzerApiUrl).toHaveBeenCalledWith('/analyze/someevents');
    req.flush(response);
  });

  it('should handle error on getSomeLoadEvents', () => {
    const expectedReq = `/api/analyze/someevents`;
    const request: GetSomeCustomEventsRequest = {
      key: 'slotKey',
      numberOfMessages: 5,
      startDate: new Date('2023-01-01T00:00:00Z'),
      readNew: true
    };

    service.getSomeLoadEvents(request).subscribe({
      next: () => fail('Expected an error, not a success'),
      error: (error) => {
        expect(error).toBeTruthy();
        expect(mockErrorHandler.handleApiError).toHaveBeenCalled();
      }
    });

    const req = httpTestingController.expectOne(expectedReq);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });

  it('should fetch some custom events', () => {
    const request: GetSomeLoadEventsRequest = {
      key: 'slotKey',
      numberOfMessages: 5,
      startDate: new Date('2023-01-01T00:00:00Z'),
      readNew: true
    };

    const response: CustomEventResponse[] = [
      {
        id: '1',
        key: 'slotKey',
        creationDateUTC: new Date('2023-01-01T00:00:00Z'),
        name: 'some-custom-event',
        description: 'some very important event',
        serializedMessage: 'seralized message of the custom event'
      }
    ];

    const expectedResult = response.map(mapCustomEventResponseToCustomEvent);

    service.getSomeCustomEvents(request).subscribe(res => {
      expect(res).toEqual(expectedResult);
    });

    const req = httpTestingController.expectOne('/api/analyze/somecustomevents');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    expect(mockUrlDefiner.combinePathWithAnalyzerApiUrl).toHaveBeenCalledWith('/analyze/somecustomevents');
    req.flush(response);
  });

  it('should handle error on getSomeCustomEvents', () => {
    const expectedReq = `/api/analyze/somecustomevents`;
    const request: GetSomeLoadEventsRequest = {
      key: 'slotKey',
      numberOfMessages: 5,
      startDate: new Date('2023-01-01T00:00:00Z'),
      readNew: true
    };

    service.getSomeCustomEvents(request).subscribe({
      next: () => fail('Expected an error, not a success'),
      error: (error) => {
        expect(error).toBeTruthy();
        expect(mockErrorHandler.handleApiError).toHaveBeenCalled();
      }
    });

    const req = httpTestingController.expectOne(expectedReq);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });

  it('should fetch slot statistics', () => {
    const key = 'slotKey';
    const response: GetSlotStatisticsResponse = {
      collectedDateUTC: new Date('2023-01-01T00:00:00Z'),
      generalStatistics: null,
      loadStatistics: null,
      customEventStatistics: null,
      lastLoadEvents: [],
      lastCustomEvents: []
    };

    const expectedResult = mapGetSlotStatisticsResponseToSlotStatistics(response);

    service.getSlotStatistics(key).subscribe(res => {
      expect(res).toEqual(expectedResult);
    });

    const req = httpTestingController.expectOne(`/api/analyze/slotstatistics/${key}`);
    expect(req.request.method).toBe('GET');
    expect(mockUrlDefiner.combinePathWithAnalyzerApiUrl).toHaveBeenCalledWith(`/analyze/slotstatistics/${key}`);
    req.flush(response);
  });

  it('should handle error on getSlotStatistics', () => {
    const key = 'slotKey';
    const expectedReq = `/api/analyze/slotstatistics/${key}`;

    service.getSlotStatistics(key).subscribe({
      next: () => fail('Expected an error, not a success'),
      error: (error) => {
        expect(error).toBeTruthy();
        expect(mockErrorHandler.handleApiError).toHaveBeenCalled();
      }
    });

    const req = httpTestingController.expectOne(expectedReq);
    req.flush('Error', { status: 400, statusText: 'Bad Request' });
  });
});
