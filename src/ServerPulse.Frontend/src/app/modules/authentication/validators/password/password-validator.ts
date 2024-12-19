import {
    AbstractControl,
    ValidationErrors,
    ValidatorFn
} from '@angular/forms';

export const passwordValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
    const password = control.value as string;

    if (!password) return { passwordInvalid: true };

    const hasMinLength = password.length >= 8;
    const hasDigit = /\d/.test(password);
    const hasUppercase = /[A-Z]/.test(password);

    const isValid = hasMinLength && hasDigit && hasUppercase;

    return isValid ? null : {
        minlength: !hasMinLength,
        digit: !hasDigit,
        uppercase: !hasUppercase,
    };
};