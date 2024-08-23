import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { CustomEventResponse, GetSomeMessagesRequest, LoadAmountStatisticsResponse, MessageAmountInRangeRequest, MessagesInRangeRangeRequest, ServerLoadResponse, StatisticsApiService, TimeSpan } from '../../../shared';
import { selectCurrentDate, selectDate, selectLastCustomStatistics, selectLastLoadStatistics, selectLastStatistics, subscribeToCustomStatistics, subscribeToLoadStatistics, subscribeToSlotStatistics } from '../../index';
import { ServerStatisticsService } from './server-statistics-service';

@Injectable({
  providedIn: 'root'
})
export class ServerStatisticsControllerService implements ServerStatisticsService {

  constructor(
    private readonly apiService: StatisticsApiService,
    private readonly store: Store
  ) { }

  getLoadEventsInDateRange(key: string, from: Date, to: Date): Observable<ServerLoadResponse[]> {
    let request: MessagesInRangeRangeRequest = {
      key: key,
      from: from,
      to: to
    }
    return this.apiService.getLoadEventsInDataRange(request);
  }
  getSomeLoadEventsFromDate(key: string, numberOfMessages: number, from: Date, readNew: boolean = false): Observable<ServerLoadResponse[]> {
    let request: GetSomeMessagesRequest = {
      key: key,
      numberOfMessages: numberOfMessages,
      startDate: from,
      readNew: readNew
    }
    return this.apiService.getSomeLoadEvents(request);
  }
  getSomeCustomEventsFromDate(key: string, numberOfMessages: number, from: Date, readNew: boolean = false): Observable<CustomEventResponse[]> {
    let request: GetSomeMessagesRequest = {
      key: key,
      numberOfMessages: numberOfMessages,
      startDate: from,
      readNew: readNew
    }
    return this.apiService.getSomeCustomEvents(request);
  }
  getWholeLoadAmountStatisticsInDays(key: string): Observable<LoadAmountStatisticsResponse[]> {
    return this.apiService.getWholeLoadAmountStatisticsInDays(key);
  }
  getLoadAmountStatisticsInRange(key: string, from: Date, to: Date, timeSpan: TimeSpan): Observable<LoadAmountStatisticsResponse[]> {
    let request: MessageAmountInRangeRequest =
    {
      key: key,
      from: from,
      to: to,
      timeSpan: timeSpan.toString()
    }
    return this.apiService.getLoadAmountStatisticsInRange(request);
  }
  getLastServerStatistics(key: string) {
    this.store.dispatch(subscribeToSlotStatistics({ slotKey: key }));
    return this.store.select(selectLastStatistics);
  }
  getLastServerLoadStatistics(key: string) {
    this.store.dispatch(subscribeToLoadStatistics({ slotKey: key }));
    return this.store.select(selectLastLoadStatistics);
  }
  getLastCustomStatistics(key: string) {
    this.store.dispatch(subscribeToCustomStatistics({ slotKey: key }));
    return this.store.select(selectLastCustomStatistics);
  }
  getCurrentLoadStatisticsDate() {
    return this.store.select(selectCurrentDate);
  }
  setCurrentLoadStatisticsDate(date: Date) {
    this.store.dispatch(selectDate({ date: date }));
  }
}