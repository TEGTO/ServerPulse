import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { ErrorHandler, URLDefiner } from '../..';

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

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  protected handleError(error: any): Observable<never> {
    const message = this.errorHandler.handleApiError(error);
    return throwError(() => new Error(message));
  }
}