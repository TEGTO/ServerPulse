/* eslint-disable @typescript-eslint/no-explicit-any */
import { TestBed } from '@angular/core/testing';
import * as signalR from '@microsoft/signalr';
import { HubConnectionState } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { BaseStatistics } from '../..';
import { ErrorHandler } from '../../../shared';
import { SignalStatisticsService } from './signal-statistics.service';

describe('SignalStatisticsService', () => {
  let service: SignalStatisticsService;
  let mockErrorHandler: jasmine.SpyObj<ErrorHandler>;
  let mockHubConnection: jasmine.SpyObj<signalR.HubConnection>;

  beforeEach(() => {
    mockErrorHandler = jasmine.createSpyObj<ErrorHandler>('ErrorHandler', ['handleHubError']);
    mockHubConnection = jasmine.createSpyObj('HubConnection', ['start', 'on', 'invoke', 'state', 'stop', 'off']);

    TestBed.configureTestingModule({
      providers: [
        SignalStatisticsService,
        { provide: ErrorHandler, useValue: mockErrorHandler },
      ],
    });

    service = TestBed.inject(SignalStatisticsService);

    spyOn(service as any, 'createNewHubConnection').and.returnValue(mockHubConnection);
  });

  afterEach(() => {
    service.ngOnDestroy();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('startConnection', () => {
    it('should start a new connection if not already present', (done) => {
      Object.defineProperty(mockHubConnection, 'state', {
        get: () => HubConnectionState.Disconnected,
        configurable: true,
        enumerable: true
      });
      const hubUrl = 'http://test-hub';
      const accessToken = 'test-token';

      mockHubConnection.start.and.returnValue(Promise.resolve());

      service.startConnection(hubUrl, accessToken).subscribe({
        complete: () => {
          expect(mockHubConnection.start).toHaveBeenCalled();
          done();
        },
      });
    });

    it('should not start connection if already connected', (done) => {
      Object.defineProperty(mockHubConnection, 'state', {
        get: () => HubConnectionState.Connected,
        configurable: true,
        enumerable: true
      });
      const hubUrl = 'http://test-hub';
      const accessToken = 'test-token';

      service.startConnection(hubUrl, accessToken).subscribe({
        complete: () => {
          expect(mockHubConnection.start).not.toHaveBeenCalled();
          done();
        },
      });
    });

    it('should not start connection if already connecting but instead wait until connected, and clear all intervals', (done) => {
      Object.defineProperty(mockHubConnection, 'state', {
        get: () => currentState,
        configurable: true,
        enumerable: true
      });

      const hubUrl = 'http://test-hub';
      const accessToken = 'test-token';
      let currentState = signalR.HubConnectionState.Connecting;

      const clearIntervalSpy = spyOn(window, 'clearInterval').and.callThrough();

      service.startConnection(hubUrl, accessToken).subscribe({
        complete: () => {
          expect(mockHubConnection.start).not.toHaveBeenCalled();
          expect(clearIntervalSpy).toHaveBeenCalled(); // Ensure waiting intervals are cleared
          done();
        },
      });

      setTimeout(() => {
        currentState = signalR.HubConnectionState.Connected;
      }, 1000);
    })

    it('should handle connection errors', (done) => {
      const hubUrl = 'http://test-hub';
      const accessToken = 'test-token';

      const error = new Error('Connection failed');
      mockHubConnection.start.and.returnValue(Promise.reject(error));

      service.startConnection(hubUrl, accessToken).subscribe({
        error: (err) => {
          expect(err).toBe(error);
          expect(mockErrorHandler.handleHubError).toHaveBeenCalledWith(error);
          done();
        },
      });
    });
  });

  describe('stopConnection', () => {
    it('should stop a connection and remove it from the map', () => {
      const hubUrl = 'http://test-hub';

      service['hubConnections'].set(hubUrl, mockHubConnection);

      service.stopConnection(hubUrl);

      expect(mockHubConnection.stop).toHaveBeenCalled();
      expect(service['hubConnections'].has(hubUrl)).toBeFalse();
    });

    it('should remove receiver from the map', () => {
      const hubUrl = 'http://test-hub';

      service['receiveStatisticsHubObservables'].set(hubUrl, new Subject());

      service.stopConnection(hubUrl);

      expect(service['receiveStatisticsHubObservables'].has(hubUrl)).toBeFalse();
    });
  });

  describe('receiveStatistics', () => {
    it('should create and return a subject for receiving statistics', () => {
      const hubUrl = 'http://test-hub';
      const mockResponse = { key: 'testKey', response: { id: 'someid', collectedDateUTC: new Date() } };
      service['hubConnections'].set(hubUrl, mockHubConnection);

      const subject = service.receiveStatistics<BaseStatistics>(hubUrl);
      expect(subject).toBeTruthy();

      subject.subscribe(message => {
        expect(message).toBeTruthy();
        expect(message.key).toContain('testKey');
        expect(message.response.id).toContain('someid');

        subject.complete();

        expect(mockHubConnection.off).toHaveBeenCalledWith('ReceiveStatistics', jasmine.any(Function));
      });

      expect(mockHubConnection.on).toHaveBeenCalledWith('ReceiveStatistics', jasmine.any(Function));

      const onCallback = mockHubConnection.on.calls.mostRecent().args[1];
      onCallback(mockResponse.key, mockResponse.response);
    });

    it('should reuse an existing subject if already present', () => {
      const hubUrl = 'http://test-hub';
      const subject = new Subject();
      service['receiveStatisticsHubObservables'].set(hubUrl, subject);

      const result = service.receiveStatistics(hubUrl);
      expect(result).toBe(subject);
      expect(mockHubConnection.on).not.toHaveBeenCalled();
    });

    it('should handle errors in the subject subscription', () => {
      const hubUrl = 'http://test-hub';
      const mockError = new Error('Test error');

      service['hubConnections'].set(hubUrl, mockHubConnection);

      const subject = service.receiveStatistics<BaseStatistics>(hubUrl);

      subject.error(mockError);

      expect(mockErrorHandler.handleHubError).toHaveBeenCalledWith(mockError);
    });

    it('should throw an exception if connection not present', () => {
      const hubUrl = 'http://test-hub';

      expect(() => service.receiveStatistics(hubUrl))
        .toThrowError(`Hub connection for ${hubUrl} is not initialized.`);
    });
  });

  describe('startListen', () => {
    it('should invoke StartListen on the hub connection', () => {
      const hubUrl = 'http://test-hub';
      const key = 'testKey';
      service['hubConnections'].set(hubUrl, mockHubConnection);

      mockHubConnection.invoke.and.returnValue(Promise.resolve());

      service.startListen(hubUrl, key);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith('StartListen', key, true);
    });

    it('should handle errors during StartListen', (done) => {
      const hubUrl = 'http://test-hub';
      const key = 'testKey';
      service['hubConnections'].set(hubUrl, mockHubConnection);

      const error = new Error('StartListen failed');
      mockHubConnection.invoke.and.returnValue(Promise.reject(error));

      service.startListen(hubUrl, key);

      setTimeout(() => {
        expect(mockErrorHandler.handleHubError).toHaveBeenCalledWith(error);
        done();
      });
    });

    it('should throw an exception if connection not present', () => {
      const hubUrl = 'http://test-hub';
      const key = 'testKey';

      expect(() => service.startListen(hubUrl, key))
        .toThrowError(`Hub connection for ${hubUrl} is not initialized.`);
    });
  });

  describe('stopListen', () => {
    it('should invoke StopListen on the hub connection', () => {
      const hubUrl = 'http://test-hub';
      const key = 'testKey';
      service['hubConnections'].set(hubUrl, mockHubConnection);

      mockHubConnection.invoke.and.returnValue(Promise.resolve());

      service.stopListen(hubUrl, key);

      expect(mockHubConnection.invoke).toHaveBeenCalledWith('StopListen', key);
    });

    it('should handle errors during StopListen', (done) => {
      const hubUrl = 'http://test-hub';
      const key = 'testKey';
      service['hubConnections'].set(hubUrl, mockHubConnection);

      const error = new Error('StopListen failed');
      mockHubConnection.invoke.and.returnValue(Promise.reject(error));

      service.stopListen(hubUrl, key);

      setTimeout(() => {
        expect(mockErrorHandler.handleHubError).toHaveBeenCalledWith(error);
        done();
      });
    });

    it('should throw an exception if connection not present', () => {
      const hubUrl = 'http://test-hub';
      const key = 'testKey';

      expect(() => service.stopListen(hubUrl, key))
        .toThrowError(`Hub connection for ${hubUrl} is not initialized.`);
    });
  });

  describe('ngOnDestroy', () => {
    it('should stop all connections and complete all subjects', () => {
      const hubUrl = 'http://test-hub';
      const subject = new Subject();

      service['hubConnections'].set(hubUrl, mockHubConnection);
      service['receiveStatisticsHubObservables'].set(hubUrl, subject);

      spyOn(subject, 'complete');

      service.ngOnDestroy();

      expect(mockHubConnection.stop).toHaveBeenCalled();
      expect(subject.complete).toHaveBeenCalled();
      expect(service['hubConnections'].size).toBe(0);
      expect(service['receiveStatisticsHubObservables'].size).toBe(0);
    });
  });
});