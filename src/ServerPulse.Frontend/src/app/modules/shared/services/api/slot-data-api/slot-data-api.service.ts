import { Injectable } from '@angular/core';
import { catchError, Observable } from 'rxjs';
import { BaseApiService, SlotDataResponse } from '../../..';

@Injectable({
  providedIn: 'root'
})
export class SlotDataApiService extends BaseApiService {

  getSlotData(key: string): Observable<SlotDataResponse> {
    return this.httpClient.get<SlotDataResponse>(this.combineWithSlotDataApiUrl(`/${key}`)).pipe(
      catchError((resp) => this.handleError(resp))
    );
  }

  private combineWithSlotDataApiUrl(subpath: string) {
    return this.urlDefiner.combineWithSlotDataApiUrl(subpath);
  }
}
