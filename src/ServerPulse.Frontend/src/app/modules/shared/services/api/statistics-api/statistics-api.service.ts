import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { CustomEventResponse, GetSomeMessagesRequest, LoadAmountStatisticsResponse, MessageAmountInRangeRequest, MessagesInRangeRangeRequest, ServerLoadResponse } from '../../../index';
import { BaseApiService } from '../base-api/base-api.service';

@Injectable({
  providedIn: 'root'
})
export class StatisticsApiService extends BaseApiService {

  getLoadEventsInDataRange(request: MessagesInRangeRangeRequest): Observable<ServerLoadResponse[]> {
    return this.httpClient.post<ServerLoadResponse[]>(this.combinePathWithStatisticsApiUrl(`/daterange`), request).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  getWholeLoadAmountStatisticsInDays(key: string): Observable<LoadAmountStatisticsResponse[]> {
    return this.httpClient.get<LoadAmountStatisticsResponse[]>(this.combinePathWithStatisticsApiUrl(`/perday/${key}`)).pipe(
      map(responses => responses.map(response => ({
        ...response,
        date: new Date(response.date)
      }))),
      catchError((resp) => this.handleError(resp))
    );
  }

  getLoadAmountStatisticsInRange(request: MessageAmountInRangeRequest): Observable<LoadAmountStatisticsResponse[]> {
    return this.httpClient.post<LoadAmountStatisticsResponse[]>(this.combinePathWithStatisticsApiUrl(`/amountrange`), request).pipe(
      map(responses => responses.map(response => ({
        ...response,
        date: new Date(response.date)
      }))),
      catchError((resp) => this.handleError(resp))
    );
  }

  getSomeLoadEvents(request: GetSomeMessagesRequest): Observable<ServerLoadResponse[]> {
    return this.httpClient.post<ServerLoadResponse[]>(this.combinePathWithStatisticsApiUrl(`/someevents`), request);
  }

  getSomeCustomEvents(request: GetSomeMessagesRequest): Observable<CustomEventResponse[]> {
    return this.httpClient.post<CustomEventResponse[]>(this.combinePathWithStatisticsApiUrl(`/somecustomevents`), request);
  }

  private combinePathWithStatisticsApiUrl(subpath: string) {
    return this.urlDefiner.combineWithStatisticsApiUrl(subpath);
  }
}