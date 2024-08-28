import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { BaseApiService, CustomEventResponse, GetSomeMessagesRequest, LoadAmountStatisticsResponse, LoadEventResponse, MessageAmountInRangeRequest, MessagesInRangeRangeRequest, transformLoadAmountStatisticsResponseDate } from '../../../index';

@Injectable({
  providedIn: 'root'
})
export class StatisticsApiService extends BaseApiService {

  getLoadEventsInDataRange(request: MessagesInRangeRangeRequest): Observable<LoadEventResponse[]> {
    return this.httpClient.post<LoadEventResponse[]>(this.combinePathWithStatisticsApiUrl(`/daterange`), request).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  getWholeLoadAmountStatisticsInDays(key: string): Observable<LoadAmountStatisticsResponse[]> {
    return this.httpClient.get<LoadAmountStatisticsResponse[]>(this.combinePathWithStatisticsApiUrl(`/perday/${key}`)).pipe(
      map(responses => responses.map(transformLoadAmountStatisticsResponseDate)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getLoadAmountStatisticsInRange(request: MessageAmountInRangeRequest): Observable<LoadAmountStatisticsResponse[]> {
    return this.httpClient.post<LoadAmountStatisticsResponse[]>(this.combinePathWithStatisticsApiUrl(`/amountrange`), request).pipe(
      map(responses => responses.map(transformLoadAmountStatisticsResponseDate)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getSomeLoadEvents(request: GetSomeMessagesRequest): Observable<LoadEventResponse[]> {
    return this.httpClient.post<LoadEventResponse[]>(this.combinePathWithStatisticsApiUrl(`/someevents`), request);
  }

  getSomeCustomEvents(request: GetSomeMessagesRequest): Observable<CustomEventResponse[]> {
    return this.httpClient.post<CustomEventResponse[]>(this.combinePathWithStatisticsApiUrl(`/somecustomevents`), request);
  }

  private combinePathWithStatisticsApiUrl(subpath: string) {
    return this.urlDefiner.combineWithStatisticsApiUrl(subpath);
  }
}