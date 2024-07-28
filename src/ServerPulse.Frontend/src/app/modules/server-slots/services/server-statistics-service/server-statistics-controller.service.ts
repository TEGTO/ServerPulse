import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ServerStatisticsResponse, StatisticsApiService } from '../../../shared';
import { ServerStatisticsService } from './server-statistics-service';

@Injectable({
  providedIn: 'root'
})
export class ServerStatisticsControllerService implements ServerStatisticsService {

  constructor(
    private readonly statisticsApi: StatisticsApiService
  ) { }

  getCurrentServerStatisticsByKey(key: string): Observable<ServerStatisticsResponse> {
    return this.statisticsApi.getCurrentServerStatisticsByKey(key);
  }
}
