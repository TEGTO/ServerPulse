import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

export function minDateValidator(minDate: Date): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        const selectedDate = new Date(control.value);
        if (selectedDate < minDate) {
            return { invalidMinDate: true };
        }
        return null;
    };
}