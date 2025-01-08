
import { HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, map, Observable } from 'rxjs';
import { CreateSlotRequest, CreateSlotResponse, GetSlotByIdResponse, GetSlotsByEmailResponse, mapCreateSlotResponseToServerSlot, mapGetSlotByIdResponseToServerSlot, mapGetSlotsByEmailResponseToServerSlot, ServerSlot, UpdateSlotRequest } from '../..';
import { BaseApiService } from '../../../shared';
@Injectable({
  providedIn: 'root'
})
export class ServerSlotApiService extends BaseApiService {
  getUserServerSlots(containsStr = ""): Observable<ServerSlot[]> {
    const params = new HttpParams().set('contains', containsStr);

    return this.httpClient.get<GetSlotsByEmailResponse[]>(this.combinePathWithServerSlotApiUrl(``), { params }).pipe(
      map((response) => response.map(mapGetSlotsByEmailResponseToServerSlot)),
      catchError((resp) => this.handleError(resp))
    );
  }

  getServerSlotById(id: string): Observable<ServerSlot> {
    return this.httpClient.get<GetSlotByIdResponse>(this.combinePathWithServerSlotApiUrl(`/${id}`)).pipe(
      map((response) => mapGetSlotByIdResponseToServerSlot(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  createServerSlot(request: CreateSlotRequest): Observable<ServerSlot> {
    return this.httpClient.post<CreateSlotResponse>(this.combinePathWithServerSlotApiUrl(``), request).pipe(
      map((response) => mapCreateSlotResponseToServerSlot(response)),
      catchError((resp) => this.handleError(resp))
    );
  }

  updateServerSlot(request: UpdateSlotRequest): Observable<HttpResponse<void>> {
    return this.httpClient.put<void>(this.combinePathWithServerSlotApiUrl(``), request, { observe: 'response' }).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }

  deleteServerSlot(id: string): Observable<HttpResponse<void>> {
    return this.httpClient.delete<void>(this.combinePathWithServerSlotApiUrl(`/${id}`), { observe: 'response' }).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }

  private combinePathWithServerSlotApiUrl(subpath: string): string {
    return this.urlDefiner.combineWithServerSlotApiUrl("/serverslot" + subpath);
  }
}
