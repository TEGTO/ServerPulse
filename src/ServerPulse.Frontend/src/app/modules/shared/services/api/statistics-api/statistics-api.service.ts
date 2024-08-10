import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { LoadAmountStatisticsInRangeRequest, LoadAmountStatisticsResponse, LoadEventsRangeRequest, ServerLoadResponse } from '../../..';
import { BaseApiService } from '../base-api/base-api.service';

@Injectable({
  providedIn: 'root'
})
export class StatisticsApiService extends BaseApiService {

  getLoadEventsInDataRange(request: LoadEventsRangeRequest): Observable<ServerLoadResponse[]> {
    return this.httpClient.post<ServerLoadResponse[]>(this.combinePathWithStatisticsApiUrl(`/daterange`), request).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  getWholeAmountStatisticsInDays(key: string): Observable<LoadAmountStatisticsResponse[]> {
    return this.httpClient.get<LoadAmountStatisticsResponse[]>(this.combinePathWithStatisticsApiUrl(`/perday/${key}`)).pipe(
      map(responses => responses.map(response => ({
        ...response,
        date: new Date(response.date)
      }))),
      catchError((resp) => this.handleError(resp))
    );
  }

  getAmountStatisticsInRange(request: LoadAmountStatisticsInRangeRequest): Observable<LoadAmountStatisticsResponse[]> {
    return this.httpClient.post<LoadAmountStatisticsResponse[]>(this.combinePathWithStatisticsApiUrl(`/amountrange`), request).pipe(
      map(responses => responses.map(response => ({
        ...response,
        date: new Date(response.date)
      }))),
      catchError((resp) => this.handleError(resp))
    );
  }

  private combinePathWithStatisticsApiUrl(subpath: string) {
    return this.urlDefiner.combineWithStatisticsApiUrl(subpath);
  }
}
