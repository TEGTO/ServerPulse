import { Injectable } from '@angular/core';
import { environment } from '../../../../../environment/environment';

@Injectable({
  providedIn: 'root'
})
export class URLDefiner {
  combineWithServerSlotApiUrl(subpath: string): string {
    return environment.api + subpath;
  }

  combineWithAuthApiUrl(subpath: string): string {
    return environment.api + subpath;
  }

  combinePathWithAnalyzerApiUrl(subpath: string): string {
    return environment.api + subpath;
  }
}
