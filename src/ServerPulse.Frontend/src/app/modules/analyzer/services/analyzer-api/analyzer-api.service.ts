import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { CustomEvent, CustomEventResponse, GetLoadAmountStatisticsInRangeRequest, GetLoadEventsInDataRangeRequest, GetSlotStatisticsResponse, GetSomeCustomEventsRequest, GetSomeLoadEventsRequest, LoadAmountStatistics, LoadAmountStatisticsResponse, LoadEvent, LoadEventResponse, mapCustomEventResponseToCustomEvent, mapGetSlotStatisticsResponseToSlotStatistics, mapLoadAmountStatisticsResponseToLoadAmountStatistics, mapLoadEventResponseToLoadEvent, SlotStatistics } from '../..';
import { BaseApiService } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class AnalyzerApiService extends BaseApiService {

  getDailyLoadStatistics(key: string): Observable<LoadAmountStatistics[]> {
    return this.httpClient.get<LoadAmountStatisticsResponse[]>(this.combinePathWithAnalyzerApiUrl(`/perday/${key}`)).pipe(
      map((response) => response.map(mapLoadAmountStatisticsResponseToLoadAmountStatistics)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getLoadAmountStatisticsInRange(request: GetLoadAmountStatisticsInRangeRequest): Observable<LoadAmountStatistics[]> {
    return this.httpClient
      .post<LoadAmountStatisticsResponse[]>(this.combinePathWithAnalyzerApiUrl(`/amountrange`), request).pipe(
        map((response) => response.map(mapLoadAmountStatisticsResponseToLoadAmountStatistics)),
        catchError((resp) => this.handleError(resp))
      );
  }

  getLoadEventsInDataRange(request: GetLoadEventsInDataRangeRequest): Observable<LoadEvent[]> {
    return this.httpClient.post<LoadEventResponse[]>(this.combinePathWithAnalyzerApiUrl(`/daterange`), request).pipe(
      map((response) => response.map(mapLoadEventResponseToLoadEvent)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getSlotStatistics(key: string): Observable<SlotStatistics> {
    return this.httpClient.get<GetSlotStatisticsResponse>(this.combinePathWithAnalyzerApiUrl(`/slotstatistics/${key}`)).pipe(
      map((response) => mapGetSlotStatisticsResponseToSlotStatistics(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getSomeLoadEvents(request: GetSomeCustomEventsRequest): Observable<LoadEvent[]> {
    return this.httpClient.post<LoadEventResponse[]>(this.combinePathWithAnalyzerApiUrl(`/someevents`), request).pipe(
      map((response) => response.map(mapLoadEventResponseToLoadEvent)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getSomeCustomEvents(request: GetSomeLoadEventsRequest): Observable<CustomEvent[]> {
    return this.httpClient.post<CustomEventResponse[]>(this.combinePathWithAnalyzerApiUrl(`/somecustomevents`), request).pipe(
      map((response) => response.map(mapCustomEventResponseToCustomEvent)),
      catchError((resp) => this.handleError(resp))
    );
  }

  private combinePathWithAnalyzerApiUrl(subpath: string): string {
    return this.urlDefiner.combinePathWithAnalyzerApiUrl("/analyze" + subpath);
  }
}
