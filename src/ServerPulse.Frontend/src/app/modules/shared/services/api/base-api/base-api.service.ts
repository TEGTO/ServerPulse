import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { throwError } from 'rxjs';
import { ErrorHandler, URLDefiner } from '../../../index';

@Injectable({
  providedIn: 'root'
})
export abstract class BaseApiService {

  protected get httpClient(): HttpClient { return this._httpClient }
  protected get urlDefiner(): URLDefiner { return this._urlDefiner }

  constructor(
    private readonly _httpClient: HttpClient,
    private readonly errorHandler: ErrorHandler,
    private readonly _urlDefiner: URLDefiner
  ) { }

  protected handleError(error: any) {
    let message = this.errorHandler.handleApiError(error);
    return throwError(() => new Error(message));
  }
}