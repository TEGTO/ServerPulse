import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { ServerStatisticsResponse } from '../../..';
import { BaseApiService } from '../base-api/base-api.service';

@Injectable({
  providedIn: 'root'
})
export class StatisticsApiService extends BaseApiService {

  getCurrentServerStatisticsByKey(key: string): Observable<ServerStatisticsResponse> {
    return this.httpClient.get<ServerStatisticsResponse>(this.combinePathWithStatisticsApiUrl(`/${key}`)).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }

  private combinePathWithStatisticsApiUrl(subpath: string) {
    return this.urlDefiner.combineWithStatisticsApiUrl(subpath);
  }
}
