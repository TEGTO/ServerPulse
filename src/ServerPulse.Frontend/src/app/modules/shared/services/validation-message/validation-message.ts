/* eslint-disable @typescript-eslint/no-explicit-any */
import { Injectable } from "@angular/core";
import { AbstractControl } from "@angular/forms";


@Injectable({
    providedIn: 'root'
})
export abstract class ValidationMessage {
    abstract getValidationMessage(input: AbstractControl<any, any>): { hasError: boolean, message: string };
}
