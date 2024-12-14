import {
    AbstractControl,
    FormGroup,
    ValidationErrors,
    ValidatorFn,
} from '@angular/forms';

export const confirmPasswordValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
    const formGroup = control.parent as FormGroup;
    if (!formGroup) return null;
    const password = formGroup.get('password')?.value;
    const passwordConfirm = control.value;
    return password === passwordConfirm ? null : { passwordNoMatch: true };
};

export const confirmPasswordValidatorGroup: ValidatorFn = (formGroup: AbstractControl): ValidationErrors | null => {
    const password = formGroup.get('password')?.value;
    const confirmPassword = formGroup.get('passwordConfirm')?.value;

    return password === confirmPassword ? null : { passwordNoMatch: true };
};