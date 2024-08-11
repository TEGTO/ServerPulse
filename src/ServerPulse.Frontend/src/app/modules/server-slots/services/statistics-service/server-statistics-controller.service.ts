import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { selectCurrentDate, selectDate, selectLastLoadStatistics, selectLastStatistics, subscribeToLoadStatistics, subscribeToSlotStatistics } from '../..';
import { LoadAmountStatisticsInRangeRequest, LoadAmountStatisticsResponse, LoadEventsRangeRequest, ServerLoadResponse, StatisticsApiService, TimeSpan } from '../../../shared';
import { ServerStatisticsService } from './server-statistics-service';

@Injectable({
  providedIn: 'root'
})
export class ServerStatisticsControllerService implements ServerStatisticsService {

  constructor(
    private readonly apiService: StatisticsApiService,
    private readonly store: Store
  ) { }

  getStatisticsInDateRange(key: string, from: Date, to: Date): Observable<ServerLoadResponse[]> {
    let request: LoadEventsRangeRequest = {
      key: key,
      from: from,
      to: to
    }
    return this.apiService.getLoadEventsInDataRange(request);
  }
  getWholeAmountStatisticsInDays(key: string): Observable<LoadAmountStatisticsResponse[]> {
    return this.apiService.getWholeAmountStatisticsInDays(key);
  }
  getAmountStatisticsInRange(key: string, from: Date, to: Date, timeSpan: TimeSpan): Observable<LoadAmountStatisticsResponse[]> {
    let request: LoadAmountStatisticsInRangeRequest =
    {
      key: key,
      from: from,
      to: to,
      timeSpan: timeSpan.toString()
    }
    return this.apiService.getAmountStatisticsInRange(request);
  }
  getLastServerStatistics(key: string) {
    this.store.dispatch(subscribeToSlotStatistics({ slotKey: key }));
    return this.store.select(selectLastStatistics);
  }
  getLastServerLoadStatistics(key: string) {
    this.store.dispatch(subscribeToLoadStatistics({ slotKey: key }));
    return this.store.select(selectLastLoadStatistics);
  }
  getCurrentLoadStatisticsDate() {
    return this.store.select(selectCurrentDate);
  }
  setCurrentLoadStatisticsDate(date: Date) {
    this.store.dispatch(selectDate({ date: date }));
  }
}