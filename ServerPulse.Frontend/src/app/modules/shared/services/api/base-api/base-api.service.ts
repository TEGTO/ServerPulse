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

  constructor(private _httpClient: HttpClient, private errorHandler: CustomErrorHandler, private _urlDefiner: URLDefiner) { }

  protected handleError(error: any) {
    let message = this.errorHandler.handleError(error);
    return throwError(() => new Error(message));
  }
}