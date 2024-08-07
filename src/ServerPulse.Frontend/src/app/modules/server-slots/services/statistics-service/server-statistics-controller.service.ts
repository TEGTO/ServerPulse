import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { LoadEventsRangeRequest, ServerLoadResponse, StatisticsApiService } from '../../../shared';
import { ServerStatisticsService } from './server-statistics-service';

@Injectable({
  providedIn: 'root'
})
export class ServerStatisticsControllerService implements ServerStatisticsService {

  constructor(
    private readonly apiService: StatisticsApiService
  ) { }

  getStatisticsInDateRange(key: string, from: Date, to: Date): Observable<ServerLoadResponse[]> {
    let request: LoadEventsRangeRequest = {
      key: key,
      from: from,
      to: to
    }
    return this.apiService.getLoadEventsInDataRange(request);
  }
}
