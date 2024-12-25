import { AbstractControl } from "@angular/forms";
import { minDateValidator } from "./minDateValidator";

describe('minDateValidator', () => {
    const minDate = new Date('2023-01-01');
    const validator = minDateValidator(minDate);

    it('should return error when the date is before the minimum date', () => {
        const control = { value: '2022-12-31' } as AbstractControl;
        expect(validator(control)).toEqual({ invalidMinDate: true });
    });

    it('should return null when the date is equal to the minimum date', () => {
        const control = { value: '2023-01-01' } as AbstractControl;
        expect(validator(control)).toBeNull();
    });

    it('should return null when the date is after the minimum date', () => {
        const control = { value: '2023-01-02' } as AbstractControl;
        expect(validator(control)).toBeNull();
    });

    it('should return null for invalid date inputs', () => {
        const control = { value: 'invalid-date' } as AbstractControl;
        expect(validator(control)).toBeNull(); // Invalid date inputs should not trigger the error
    });
});