import { HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { CreateServerSlotRequest, mapServerSlotResponseToServerSlot, ServerSlotResponse, UpdateServerSlotRequest } from '../..';
import { BaseApiService } from '../../../shared';

@Injectable({
  providedIn: 'root'
})
export class ServerSlotApiService extends BaseApiService {
  getUserServerSlots(containsStr = ""): Observable<ServerSlotResponse[]> {
    const params = new HttpParams().set('contains', containsStr);

    return this.httpClient.get<ServerSlotResponse[]>(this.combinePathWithServerSlotApiUrl(``), { params }).pipe(
      map((response) => response.map(mapServerSlotResponseToServerSlot)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getServerSlotById(id: string): Observable<ServerSlotResponse> {
    return this.httpClient.get<ServerSlotResponse>(this.combinePathWithServerSlotApiUrl(`/${id}`)).pipe(
      map((response) => mapServerSlotResponseToServerSlot(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  createServerSlot(request: CreateServerSlotRequest): Observable<ServerSlotResponse> {
    return this.httpClient.post<ServerSlotResponse>(this.combinePathWithServerSlotApiUrl(``), request).pipe(
      map((response) => mapServerSlotResponseToServerSlot(response)),
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
    return this.urlDefiner.combineWithServerSlotApiUrl("/serverslot" + subpath);
  }
}
