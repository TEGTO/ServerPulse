import { AbstractControl } from "@angular/forms";
import { notEmptyString } from "./emptyStringValidator";

describe('notEmptyString', () => {
    it('should return error when value is an empty string', () => {
        const control = { value: '' } as AbstractControl;
        expect(notEmptyString(control)).toEqual({ notEmptyString: true });
    });

    it('should return error when value is a string with only spaces', () => {
        const control = { value: '   ' } as AbstractControl;
        expect(notEmptyString(control)).toEqual({ notEmptyString: true });
    });

    it('should return null for a non-empty string', () => {
        const control = { value: 'valid' } as AbstractControl;
        expect(notEmptyString(control)).toBeNull();
    });

    it('should return null for non-string values', () => {
        const control = { value: 123 } as AbstractControl;
        expect(notEmptyString(control)).toBeNull();
    });
});