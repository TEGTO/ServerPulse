import { HttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { CustomErrorHandler } from '../../error-handler/custom-error-handler.service';
import { URLDefiner } from '../../url-definer/url-definer.service';
import { BaseApiService } from './base-api.service';

describe('BaseApiService', () => {
  let service: BaseApiService;
  let mockHttpClient: jasmine.SpyObj<HttpClient>;
  let mockErrorHandler: jasmine.SpyObj<CustomErrorHandler>;
  let mockUrlDefiner: jasmine.SpyObj<URLDefiner>;

  beforeEach(() => {
    mockHttpClient = jasmine.createSpyObj('HttpClient', ['get', 'post', 'put', 'delete']);
    mockErrorHandler = jasmine.createSpyObj('CustomErrorHandler', ['handleApiError']);
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
    expect(service['httpClient']).toBe(mockHttpClient);
  });

  it('should return CustomErrorHandler instance', () => {
    expect(service['errorHandler']).toBe(mockErrorHandler);
  });

  it('should return URLDefiner instance', () => {
    expect(service['urlDefiner']).toBe(mockUrlDefiner);
  });

  it('should handle error using CustomErrorHandler', () => {
    const error = new Error('Test error');
    service['handleError'](error).subscribe({
      error: (err) => {
      }
    });
    expect(mockErrorHandler.handleApiError).toHaveBeenCalledWith(error);
  });
});