import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { throwError } from 'rxjs';
import { CustomErrorHandler, URLDefiner } from '../../../index';

@Injectable({
  providedIn: 'root'
})
export class BaseApiService {

  protected get httpClient(): HttpClient { return this._httpClient }
  protected get urlDefiner(): URLDefiner { return this._urlDefiner }

  constructor(
    private readonly _httpClient: HttpClient,
    private readonly errorHandler: CustomErrorHandler,
    private readonly _urlDefiner: URLDefiner
  ) { }

  protected handleError(error: any) {
    let message = this.errorHandler.handleApiError(error);
    return throwError(() => new Error(message));
  }
}