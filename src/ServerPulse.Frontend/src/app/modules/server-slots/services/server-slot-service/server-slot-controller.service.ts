import { Injectable } from '@angular/core';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { createServerSlot, deleteServerSlot, getServerSlots, getServerSlotsWithString, selectServerSlots, updateServerSlot } from '../..';
import { CreateServerSlotRequest, ServerSlot, UpdateServerSlotRequest } from '../../../shared';
import { ServerSlotService } from './server-slot-service';

@Injectable({
  providedIn: 'root'
})
export class ServerSlotControllerService implements ServerSlotService {

  constructor(
    private readonly store: Store
  ) { }

  getServerSlots(): Observable<ServerSlot[]> {
    this.store.dispatch(getServerSlots());
    return this.store.select(selectServerSlots);
  }
  getServerSlotsWithString(str: string): Observable<ServerSlot[]> {
    this.store.dispatch(getServerSlotsWithString({ str: str }));
    return this.store.select(selectServerSlots);
  }
  createServerSlot(request: CreateServerSlotRequest) {
    this.store.dispatch(createServerSlot({ createRequest: request }));
  }
  updateServerSlot(request: UpdateServerSlotRequest) {
    this.store.dispatch(updateServerSlot({ updateRequest: request }));
  }
  deleteServerSlot(id: string) {
    this.store.dispatch(deleteServerSlot({ id: id }));
  }
}