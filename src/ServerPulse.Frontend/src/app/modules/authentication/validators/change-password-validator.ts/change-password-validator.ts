import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

export const changePasswordValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
    const password = control.value as string;

    if (!password) return null;

    const hasMinLength = password.length >= 8;
    const hasNonAlphanumeric = /[^a-zA-Z0-9]/.test(password);
    const hasDigit = /\d/.test(password);
    const hasUppercase = /[A-Z]/.test(password);

    const isValid = hasMinLength && hasNonAlphanumeric && hasDigit && hasUppercase;

    return isValid ? null : {
        minlength: !hasMinLength,
        nonAlphanumeric: !hasNonAlphanumeric,
        digit: !hasDigit,
        uppercase: !hasUppercase,
    };
};