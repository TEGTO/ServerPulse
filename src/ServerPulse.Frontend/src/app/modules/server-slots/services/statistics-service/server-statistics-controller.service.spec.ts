import { TestBed } from '@angular/core/testing';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { GetSomeMessagesRequest, MessageAmountInRangeRequest, MessagesInRangeRangeRequest, StatisticsApiService, TimeSpan } from '../../../shared';
import { selectDate, subscribeToLoadStatistics, subscribeToSlotStatistics } from '../../index';
import { ServerStatisticsControllerService } from './server-statistics-controller.service';

describe('ServerStatisticsControllerService', () => {
  let service: ServerStatisticsControllerService;
  let apiService: jasmine.SpyObj<StatisticsApiService>;
  let store: jasmine.SpyObj<Store>;

  beforeEach(() => {
    const apiServiceSpy = jasmine.createSpyObj('StatisticsApiService', [
      'getLoadEventsInDataRange',
      'getSomeEventsAfterDate',
      'getWholeAmountStatisticsInDays',
      'getAmountStatisticsInRange'
    ]);

    const storeSpy = jasmine.createSpyObj('Store', ['dispatch', 'select']);

    TestBed.configureTestingModule({
      providers: [
        ServerStatisticsControllerService,
        { provide: StatisticsApiService, useValue: apiServiceSpy },
        { provide: Store, useValue: storeSpy }
      ]
    });

    service = TestBed.inject(ServerStatisticsControllerService);
    apiService = TestBed.inject(StatisticsApiService) as jasmine.SpyObj<StatisticsApiService>;
    store = TestBed.inject(Store) as jasmine.SpyObj<Store>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call getLoadEventsInDataRange with correct request in getLoadEventsInDateRange', () => {
    const key = 'testKey';
    const from = new Date('2023-01-01');
    const to = new Date('2023-01-31');
    const mockResponse = [{
      id: "",
      key: "",
      creationDateUTC: new Date(),
      endpoint: "",
      method: "",
      statusCode: 200,
      duration: new TimeSpan(0, 0, 0, 0),
      timestampUTC: new Date(),
    }];

    apiService.getLoadEventsInDataRange.and.returnValue(of(mockResponse));

    service.getLoadEventsInDateRange(key, from, to).subscribe(response => {
      expect(response).toEqual(mockResponse);
    });

    const expectedRequest: MessagesInRangeRangeRequest = { key, from, to };
    expect(apiService.getLoadEventsInDataRange).toHaveBeenCalledWith(expectedRequest);
  });

  it('should call apiService.getSomeEventsAfterDate with correct request in getSomeLoadEventsFromDate', () => {
    const key = 'testKey';
    const from = new Date('2023-01-01');
    const numberOfMessages = 5;
    const readNew = true;
    const mockResponse = [{
      id: "",
      key: "",
      creationDateUTC: new Date(),
      endpoint: "",
      method: "",
      statusCode: 200,
      duration: new TimeSpan(0, 0, 0, 0),
      timestampUTC: new Date(),
    }];

    apiService.getSomeCustomEvents.and.returnValue(of(mockResponse));

    service.getSomeLoadEventsFromDate(key, numberOfMessages, from, readNew).subscribe(response => {
      expect(response).toEqual(mockResponse);
    });

    const expectedRequest: GetSomeMessagesRequest = { key, numberOfMessages, startDate: from, readNew };
    expect(apiService.getSomeCustomEvents).toHaveBeenCalledWith(expectedRequest);
  });

  it('should call apiService.getWholeAmountStatisticsInDays with correct key in getWholeAmountStatisticsInDays', () => {
    const key = 'testKey';
    const mockResponse = [{
      amountOfEvents: 10,
      date: new Date(),
    }];

    apiService.getWholeLoadAmountStatisticsInDays.and.returnValue(of(mockResponse));

    service.getWholeLoadAmountStatisticsInDays(key).subscribe(response => {
      expect(response).toEqual(mockResponse);
    });

    expect(apiService.getWholeLoadAmountStatisticsInDays).toHaveBeenCalledWith(key);
  });

  it('should call apiService.getAmountStatisticsInRange with correct request in getAmountStatisticsInRange', () => {
    const key = 'testKey';
    const from = new Date('2023-01-01');
    const to = new Date('2023-01-31');
    const timeSpan = new TimeSpan(24, 0, 0, 0);
    const mockResponse = [{
      amountOfEvents: 10,
      date: new Date(),
    }];

    apiService.getLoadAmountStatisticsInRange.and.returnValue(of(mockResponse));

    service.getLoadAmountStatisticsInRange(key, from, to, timeSpan).subscribe(response => {
      expect(response).toEqual(mockResponse);
    });
    const expectedRequest: MessageAmountInRangeRequest = { key, from, to, timeSpan: timeSpan.toString() };
    expect(apiService.getLoadAmountStatisticsInRange).toHaveBeenCalledWith(expectedRequest);
  });

  it('should dispatch subscribeToSlotStatistics and select selectLastStatistics in getLastServerStatistics', () => {
    const key = 'testKey';
    const mockStatistics = null;

    store.select.and.returnValue(of(mockStatistics));

    service.getLastServerStatistics(key).subscribe(response => {
      expect(response).toEqual(mockStatistics);
    });

    expect(store.dispatch).toHaveBeenCalledWith(subscribeToSlotStatistics({ slotKey: key }));
  });

  it('should dispatch subscribeToLoadStatistics and select selectLastLoadStatistics in getLastServerLoadStatistics', () => {
    const key = 'testKey';
    const mockStatistics = null;

    store.select.and.returnValue(of(mockStatistics));

    service.getLastServerLoadStatistics(key).subscribe(response => {
      expect(response).toEqual(mockStatistics);
    });

    expect(store.dispatch).toHaveBeenCalledWith(subscribeToLoadStatistics({ slotKey: key }));
  });

  it('should select selectCurrentDate in getCurrentLoadStatisticsDate', () => {
    const mockDate = new Date('2023-01-01');

    store.select.and.returnValue(of(mockDate));

    service.getCurrentLoadStatisticsDate().subscribe(response => {
      expect(response).toEqual(mockDate);
    });

  });

  it('should dispatch selectDate in setCurrentLoadStatisticsDate', () => {
    const mockDate = new Date('2023-01-01');

    service.setCurrentLoadStatisticsDate(mockDate);

    expect(store.dispatch).toHaveBeenCalledWith(selectDate({ date: mockDate }));
  });
})