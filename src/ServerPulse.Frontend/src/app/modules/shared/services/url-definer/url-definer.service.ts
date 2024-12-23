import { Injectable } from '@angular/core';
import { environment } from '../../../../../environment/environment';

@Injectable({
  providedIn: 'root'
})
export class URLDefiner {
  combineWithServerSlotApiUrl(subpath: string): string {
    return environment.api + "/serverslot" + subpath;
  }
  combineWithAuthApiUrl(subpath: string): string {
    return environment.authApi + subpath;
  }
  combineWithStatisticsApiUrl(subpath: string): string {
    return environment.api + "/analyze" + subpath;
  }
  combineWithSlotDataApiUrl(subpath: string): string {
    return environment.api + "/slotdata" + subpath;
  }
}
