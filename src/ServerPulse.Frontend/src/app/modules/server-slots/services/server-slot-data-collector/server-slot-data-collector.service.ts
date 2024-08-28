import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { SlotDataApiService, SlotDataResponse } from '../../../shared';
import { ServerSlotDataCollector } from './server-slot-data-collector';

@Injectable({
  providedIn: 'root'
})
export class ServerSlotDataCollectorService implements ServerSlotDataCollector {

  constructor(
    private readonly apiService: SlotDataApiService
  ) { }

  getServerSlotData(slotKey: string): Observable<SlotDataResponse> {
    return this.apiService.getSlotData(slotKey);
  }
}
