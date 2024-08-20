import { TimeSpan } from "./timespan";

describe('TimeSpan', () => {

    it('should correctly calculate totalMilliseconds from hours, minutes, seconds, and milliseconds', () => {
        const timeSpan = new TimeSpan(1, 1, 1, 500);
        expect(timeSpan['totalMilliseconds']).toBe((1 * 60 * 60 * 1000) + (1 * 60 * 1000) + (1 * 1000) + 500);
    });

    it('should create a TimeSpan from milliseconds', () => {
        const milliseconds = 123456;
        const timeSpan = TimeSpan.fromMilliseconds(milliseconds);
        expect(timeSpan['totalMilliseconds']).toBe(milliseconds);
    });

    it('should create a TimeSpan from a string in "hh:mm:ss" format', () => {
        const timeSpan = TimeSpan.fromString('01:02:03');
        expect(timeSpan['totalMilliseconds']).toBe((1 * 60 * 60 * 1000) + (2 * 60 * 1000) + (3 * 1000));
    });

    it('should create a TimeSpan from a string in "mm:ss" format', () => {
        const timeSpan = TimeSpan.fromString('02:03');
        expect(timeSpan['totalMilliseconds']).toBe((2 * 60 * 1000) + (3 * 1000));
    });

    it('should throw an error for an invalid format', () => {
        expect(() => TimeSpan.fromString('invalid')).toThrowError('Invalid TimeSpan format');
    });

    it('should return the correct hours, minutes, seconds, and milliseconds', () => {
        const timeSpan = new TimeSpan(1, 2, 3, 456);
        expect(timeSpan.hours).toBe(1);
        expect(timeSpan.minutes).toBe(2);
        expect(timeSpan.seconds).toBe(3);
        expect(timeSpan.milliseconds).toBe(456);
    });

    it('should return a string in "hh:mm:ss" format', () => {
        const timeSpan = new TimeSpan(1, 2, 3, 456);
        expect(timeSpan.toString()).toBe('01:02:03');
    });

    it('should correctly pad hours, minutes, and seconds with leading zeros', () => {
        const timeSpan = new TimeSpan(0, 0, 5);
        expect(timeSpan.toString()).toBe('00:00:05');
    });

});