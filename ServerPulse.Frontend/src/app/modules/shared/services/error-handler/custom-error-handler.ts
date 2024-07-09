import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export abstract class CustomErrorHandler {

  abstract handleError(error: any): string;
}
