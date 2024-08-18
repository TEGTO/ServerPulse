import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { CreateServerSlotRequest, ServerSlotResponse, UpdateServerSlotRequest } from '../../../index';
import { BaseApiService } from '../base-api/base-api.service';

@Injectable({
  providedIn: 'root'
})
export class ServerSlotApiService extends BaseApiService {

  getUserServerSlots(): Observable<ServerSlotResponse[]> {
    return this.httpClient.get<ServerSlotResponse[]>(this.combinePathWithServerSlotApiUrl(``)).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  getServerSlotById(id: string): Observable<ServerSlotResponse> {
    return this.httpClient.get<ServerSlotResponse>(this.combinePathWithServerSlotApiUrl(`/${id}`)).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  getUserServerSlotsWithString(str: string): Observable<ServerSlotResponse[]> {
    return this.httpClient.get<ServerSlotResponse[]>(this.combinePathWithServerSlotApiUrl(`/contains/${str}`)).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  createServerSlot(request: CreateServerSlotRequest): Observable<ServerSlotResponse> {
    return this.httpClient.post<ServerSlotResponse>(this.combinePathWithServerSlotApiUrl(``), request).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  updateServerSlot(request: UpdateServerSlotRequest) {
    return this.httpClient.put(this.combinePathWithServerSlotApiUrl(``), request).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  deleteServerSlot(id: string) {
    return this.httpClient.delete(this.combinePathWithServerSlotApiUrl(`/${id}`)).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }
  private combinePathWithServerSlotApiUrl(subpath: string) {
    return this.urlDefiner.combineWithServerSlotApiUrl(subpath);
  }
}