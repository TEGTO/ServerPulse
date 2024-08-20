import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { GetSomeLoadEventsRequest, LoadAmountStatisticsInRangeRequest, LoadAmountStatisticsResponse, LoadEventsRangeRequest, ServerLoadResponse, TimeSpan, URLDefiner } from '../../../index';
import { StatisticsApiService } from './statistics-api.service';

describe('StatisticsApiService', () => {
  let service: StatisticsApiService;
  let httpTestingController: HttpTestingController;
  let mockUrlDefiner: jasmine.SpyObj<URLDefiner>;

  beforeEach(() => {
    mockUrlDefiner = jasmine.createSpyObj<URLDefiner>('URLDefiner', ['combineWithStatisticsApiUrl']);
    mockUrlDefiner.combineWithStatisticsApiUrl.and.callFake((subpath: string) => `/api/statistics${subpath}`);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        StatisticsApiService,
        { provide: URLDefiner, useValue: mockUrlDefiner }
      ]
    });

    service = TestBed.inject(StatisticsApiService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should get load events in date range', () => {
    const request: LoadEventsRangeRequest = { key: 'testKey', from: new Date(), to: new Date() };
    const expectedUrl = '/api/statistics/daterange';
    const response: ServerLoadResponse[] = [
      {
        id: '1',
        key: 'testKey',
        endpoint: '/api/test',
        method: 'GET',
        statusCode: 200,
        duration: new TimeSpan(0, 0, 0, 0),
        timestampUTC: new Date(),
        creationDateUTC: new Date()
      }
    ];

    service.getLoadEventsInDataRange(request).subscribe(res => {
      expect(res).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedUrl);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithStatisticsApiUrl).toHaveBeenCalledWith('/daterange');
    req.flush(response);
  });

  it('should get whole amount statistics in days', () => {
    const key = 'testKey';
    const expectedUrl = `/api/statistics/perday/${key}`;
    const response: LoadAmountStatisticsResponse[] = [{ amountOfEvents: 10, date: new Date() }];

    service.getWholeAmountStatisticsInDays(key).subscribe(res => {
      expect(res).toEqual(response);
      expect(res[0].date instanceof Date).toBeTrue();
    });

    const req = httpTestingController.expectOne(expectedUrl);
    expect(req.request.method).toBe('GET');
    expect(mockUrlDefiner.combineWithStatisticsApiUrl).toHaveBeenCalledWith(`/perday/${key}`);
    req.flush(response);
  });

  it('should get amount statistics in range', () => {
    const request: LoadAmountStatisticsInRangeRequest = { key: 'testKey', from: new Date(), to: new Date(), timeSpan: '1d' };
    const expectedUrl = '/api/statistics/amountrange';
    const response: LoadAmountStatisticsResponse[] = [{ amountOfEvents: 10, date: new Date() }];

    service.getAmountStatisticsInRange(request).subscribe(res => {
      expect(res).toEqual(response);
      expect(res[0].date instanceof Date).toBeTrue();
    });

    const req = httpTestingController.expectOne(expectedUrl);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithStatisticsApiUrl).toHaveBeenCalledWith('/amountrange');
    req.flush(response);
  });

  it('should get some events after a date', () => {
    const request: GetSomeLoadEventsRequest = { key: 'testKey', numberOfMessages: 5, startDate: new Date(), readNew: false };
    const expectedUrl = '/api/statistics/someevents';
    const response: ServerLoadResponse[] = [
      {
        id: '1',
        key: 'testKey',
        endpoint: '/api/test',
        method: 'GET',
        statusCode: 200,
        duration: new TimeSpan(0, 0, 0, 0),
        timestampUTC: new Date(),
        creationDateUTC: new Date()
      }
    ];

    service.getSomeEventsAfterDate(request).subscribe(res => {
      expect(res).toEqual(response);
    });

    const req = httpTestingController.expectOne(expectedUrl);
    expect(req.request.method).toBe('POST');
    expect(mockUrlDefiner.combineWithStatisticsApiUrl).toHaveBeenCalledWith('/someevents');
    req.flush(response);
  });

  it('should handle error response', () => {
    const errorResponse = { status: 404, statusText: 'Not Found' };
    const expectedUrl = `/api/statistics/daterange`;

    service.getLoadEventsInDataRange({ key: 'testKey', from: new Date(), to: new Date() }).subscribe(
      () => fail('Expected an error, but got a success response'),
      (error) => {
        expect(error).toBeTruthy();
      }
    );

    const req = httpTestingController.expectOne(expectedUrl);
    req.flush('Error', errorResponse);
  });
});