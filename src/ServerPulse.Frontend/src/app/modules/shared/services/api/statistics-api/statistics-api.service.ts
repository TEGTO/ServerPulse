import { Injectable } from '@angular/core';
import { BaseApiService } from '../base-api/base-api.service';

@Injectable({
  providedIn: 'root'
})
export class StatisticsApiService extends BaseApiService {

  private combinePathWithStatisticsApiUrl(subpath: string) {
    return this.urlDefiner.combineWithStatisticsApiUrl(subpath);
  }
}
