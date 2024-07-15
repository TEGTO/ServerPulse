import { Injectable } from '@angular/core';
import { environment } from '../../../../../environment/environment';
import { URLDefiner } from './url-definer';

@Injectable({
  providedIn: 'root'
})
export class URLDefinerService extends URLDefiner {
  override combineWithServerSlotApiUrl(subpath: string): string {
    throw new Error('Method not implemented.');
  }
  override combineWithAuthApiUrl(subpath: string): string {
    return environment.api + "/serverslot" + subpath;
  }
}