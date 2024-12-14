import { AbstractControl, ValidationErrors } from "@angular/forms";

export function noSpaces(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (typeof value === 'string' && /\s/.test(value)) {
        return { noSpaces: true };
    }
    return null;
}