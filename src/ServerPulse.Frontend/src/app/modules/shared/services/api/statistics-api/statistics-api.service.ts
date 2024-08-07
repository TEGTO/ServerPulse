import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { LoadEventsRangeRequest, ServerLoadResponse } from '../../..';
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

  private combinePathWithStatisticsApiUrl(subpath: string) {
    return this.urlDefiner.combineWithStatisticsApiUrl(subpath);
  }
}
