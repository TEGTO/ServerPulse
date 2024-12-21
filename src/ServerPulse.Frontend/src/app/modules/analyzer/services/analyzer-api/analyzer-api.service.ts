import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { CustomEvent, CustomEventResponse, GetSomeMessagesRequest, LoadAmountStatistics, LoadAmountStatisticsResponse, LoadEvent, LoadEventResponse, mapCustomEventResponseToCustomEvent, mapLoadAmountStatisticsResponseToLoadAmountStatistics, mapLoadEventResponseToLoadEvent, mapSlotStatisticsResponseToSlotStatistics, MessageAmountInRangeRequest, MessagesInRangeRangeRequest, SlotStatistics, SlotStatisticsResponse } from '../..';
import { BaseApiService } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class AnalyzerApiService extends BaseApiService {

  getLoadEventsInDataRange(request: MessagesInRangeRangeRequest): Observable<LoadEvent[]> {
    return this.httpClient.post<LoadEventResponse[]>(this.combinePathWithAnalyzerApiUrl(`/daterange`), request).pipe(
      map((response) => response.map(mapLoadEventResponseToLoadEvent)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getDailyLoadStatistics(key: string): Observable<LoadAmountStatistics[]> {
    return this.httpClient.get<LoadAmountStatisticsResponse[]>(this.combinePathWithAnalyzerApiUrl(`/perday/${key}`)).pipe(
      map((response) => response.map(mapLoadAmountStatisticsResponseToLoadAmountStatistics)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getLoadAmountStatisticsInRange(request: MessageAmountInRangeRequest): Observable<LoadAmountStatistics[]> {
    return this.httpClient
      .post<LoadAmountStatisticsResponse[]>(this.combinePathWithAnalyzerApiUrl(`/amountrange`), request).pipe(
        map((response) => response.map(mapLoadAmountStatisticsResponseToLoadAmountStatistics)),
        catchError((resp) => this.handleError(resp))
      );
  }
  getSomeLoadEvents(request: GetSomeMessagesRequest): Observable<LoadEvent[]> {
    return this.httpClient.post<LoadEventResponse[]>(this.combinePathWithAnalyzerApiUrl(`/someevents`), request).pipe(
      map((response) => response.map(mapLoadEventResponseToLoadEvent)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getSomeCustomEvents(request: GetSomeMessagesRequest): Observable<CustomEvent[]> {
    return this.httpClient.post<CustomEventResponse[]>(this.combinePathWithAnalyzerApiUrl(`/somecustomevents`), request).pipe(
      map((response) => response.map(mapCustomEventResponseToCustomEvent)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getSlotStatistics(key: string): Observable<SlotStatistics> {
    return this.httpClient.get<SlotStatisticsResponse>(this.combinePathWithAnalyzerApiUrl(`/slotstatistics/${key}`)).pipe(
      map((response) => mapSlotStatisticsResponseToSlotStatistics(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  private combinePathWithAnalyzerApiUrl(subpath: string) {
    return this.urlDefiner.combinePathWithAnalyzerApiUrl("/analyze" + subpath);
  }
}
