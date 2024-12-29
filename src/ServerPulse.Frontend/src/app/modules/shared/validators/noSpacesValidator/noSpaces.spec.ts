import { AbstractControl } from "@angular/forms";
import { noSpaces } from "./noSpaces";

describe('noSpaces', () => {
    it('should return error when value contains spaces', () => {
        const control = { value: 'has spaces' } as AbstractControl;
        expect(noSpaces(control)).toEqual({ noSpaces: true });
    });

    it('should return error when value is only spaces', () => {
        const control = { value: '   ' } as AbstractControl;
        expect(noSpaces(control)).toEqual({ noSpaces: true });
    });

    it('should return null when value does not contain spaces', () => {
        const control = { value: 'noSpacesHere' } as AbstractControl;
        expect(noSpaces(control)).toBeNull();
    });

    it('should return null for non-string values', () => {
        const control = { value: 123 } as AbstractControl;
        expect(noSpaces(control)).toBeNull();
    });
});