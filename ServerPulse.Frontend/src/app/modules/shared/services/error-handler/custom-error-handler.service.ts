import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CustomErrorHandler {

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
