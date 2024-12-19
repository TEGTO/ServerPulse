import { FormControl } from "@angular/forms";
import { passwordValidator } from "../..";

describe('passwordValidator', () => {
    it('should return null for a valid password', () => {
        const control = new FormControl('Valid123');

        const result = passwordValidator(control);

        expect(result).toBeNull();
    });

    it('should return an error if the password is null or empty', () => {
        const control = new FormControl('');

        const result = passwordValidator(control);

        expect(result).toEqual({ passwordInvalid: true });
    });

    it('should return an error if the password is less than 8 characters', () => {
        const control = new FormControl('Short1');

        const result = passwordValidator(control);

        expect(result).toEqual({ minlength: true, digit: false, uppercase: false });
    });

    it('should return an error if the password does not contain a digit', () => {
        const control = new FormControl('NoDigits');

        const result = passwordValidator(control);

        expect(result).toEqual({ minlength: false, digit: true, uppercase: false });
    });

    it('should return an error if the password does not contain an uppercase letter', () => {
        const control = new FormControl('nouppercase1');

        const result = passwordValidator(control);

        expect(result).toEqual({ minlength: false, digit: false, uppercase: true });
    });

    it('should return multiple errors if the password does not meet multiple criteria', () => {
        const control = new FormControl('short');

        const result = passwordValidator(control);

        expect(result).toEqual({
            minlength: true,
            digit: true,
            uppercase: true,
        });
    });
});