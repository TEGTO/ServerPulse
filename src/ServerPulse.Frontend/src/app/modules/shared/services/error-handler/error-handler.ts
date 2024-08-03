import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export abstract class ErrorHandler {
    abstract handleApiError(error: any): string;
    abstract handleHubError(error: any): string;
}
