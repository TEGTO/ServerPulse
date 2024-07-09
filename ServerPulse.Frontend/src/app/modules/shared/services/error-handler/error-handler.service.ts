import { Injectable } from '@angular/core';
import { CustomErrorHandler } from './custom-error-handler';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService implements CustomErrorHandler {

  handleError(error: any): string {
    let errorMessage = 'An unknown error occurred!';
    if (error.error) {
      if (error.error.messages)
        errorMessage = error.error.messages.join('\n');
    } else if (error.message) {
      errorMessage = error.message;
    }
    console.log(errorMessage);
    return errorMessage;
  }
}