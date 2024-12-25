import { LOCALE_ID } from "@angular/core";
import { TestBed } from "@angular/core/testing";
import { LocalizedDatePipe } from "./localizedDatePipe";

describe('LocalizedDatePipe', () => {
    let pipe: LocalizedDatePipe;

    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                LocalizedDatePipe,
                { provide: LOCALE_ID, useValue: 'en-US' },
            ],
        });

        pipe = TestBed.inject(LocalizedDatePipe);
    });

    it('should return null for null or undefined input', () => {
        expect(pipe.transform(null)).toBeNull();
        expect(pipe.transform(undefined)).toBeNull();
    });

    it('should return null for invalid date input', () => {
        expect(pipe.transform('invalid-date')).toBeNull();
    });

    it('should format a Date object correctly', () => {
        const date = new Date('2023-12-25T15:30:00Z');
        const result = pipe.transform(date);
        expect(result).toBe(date.toLocaleDateString(undefined, {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: 'numeric',
            minute: 'numeric'
        }));
    });

    it('should format a valid date string correctly', () => {
        const dateString = '2023-12-25T15:30:00Z';
        const result = pipe.transform(dateString);
        const expectedDate = new Date(dateString).toLocaleDateString(undefined, {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: 'numeric',
            minute: 'numeric'
        });
        expect(result).toBe(expectedDate);
    });

    it('should format a valid timestamp correctly', () => {
        const timestamp = 1671966600000; // Equivalent to '2023-12-25T15:30:00Z'
        const result = pipe.transform(timestamp);
        const expectedDate = new Date(timestamp).toLocaleDateString(undefined, {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: 'numeric',
            minute: 'numeric'
        });
        expect(result).toBe(expectedDate);
    });

    it('should respect the provided locale', () => {
        const customPipe = new LocalizedDatePipe('fr-FR');
        const date = new Date('2023-12-25T15:30:00Z');
        const result = customPipe.transform(date);
        expect(result).toBe('25/12/2023, 17:30');
    });
});