import { AbstractControl, ValidationErrors } from "@angular/forms";

export function notEmptyString(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (typeof value === 'string' && value.trim().length === 0) {
        return { notEmptyString: true };
    }
    return null;
}