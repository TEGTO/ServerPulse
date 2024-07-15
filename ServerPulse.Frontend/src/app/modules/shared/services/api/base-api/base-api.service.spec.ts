import { HttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { CustomErrorHandler } from '../../error-handler/custom-error-handler.service';
import { URLDefiner } from '../../url-definer/url-definer.service';
import { BaseApiService } from './base-api.service';

describe('BaseApiService', () => {
  var service: BaseApiService;
  var mockHttpClient: jasmine.SpyObj<HttpClient>;
  var mockErrorHandler: jasmine.SpyObj<CustomErrorHandler>;
  var mockUrlDefiner: jasmine.SpyObj<URLDefiner>;

  beforeEach(() => {
    mockHttpClient = jasmine.createSpyObj('HttpClient', ['get', 'post', 'put', 'delete']);
    mockErrorHandler = jasmine.createSpyObj('CustomErrorHandler', ['handleError']);
    mockUrlDefiner = jasmine.createSpyObj('URLDefiner', ['combineWithApartmentApiUrl']);

    TestBed.configureTestingModule({
      providers: [
        BaseApiService,
        { provide: HttpClient, useValue: mockHttpClient },
        { provide: CustomErrorHandler, useValue: mockErrorHandler },
        { provide: URLDefiner, useValue: mockUrlDefiner }
      ]
    });

    service = TestBed.inject(BaseApiService);
  });

  it('should return HttpClient instance', () => {
    expect(service['getHttpClient']()).toBe(mockHttpClient);
  });

  it('should return CustomErrorHandler instance', () => {
    expect(service['getErrorHandler']()).toBe(mockErrorHandler);
  });

  it('should combine path with API URL', () => {
    const path = '/test-path';
    mockUrlDefiner.combineWithAuthApiUrl.and.returnValue('http://api.example.com/test-path');
    const combinedUrl = service['combinePathWithApartmentApiUrl'](path);
    expect(combinedUrl).toBe('http://api.example.com/test-path');
    expect(mockUrlDefiner.combineWithAuthApiUrl).toHaveBeenCalledWith(path);
  });

  it('should handle error using CustomErrorHandler', () => {
    const error = new Error('Test error');
    service['handleError'](error).subscribe({
      error: (err) => {
      }
    });
    expect(mockErrorHandler.handleError).toHaveBeenCalledWith(error);
  });
});