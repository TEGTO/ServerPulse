export class TimeSpan {
    private readonly totalMilliseconds: number;

    constructor(hours = 0, minutes = 0, seconds = 0, milliseconds = 0) {
        this.totalMilliseconds = ((hours * 60 * 60) + (minutes * 60) + seconds) * 1000 + milliseconds;
    }

    static fromMilliseconds(milliseconds: number): TimeSpan {
        return new TimeSpan(0, 0, 0, milliseconds);
    }

    static fromString(timeSpanString: string): TimeSpan {
        const parts = timeSpanString.split(':').map(part => parseFloat(part));
        if (parts.length === 3) {
            const [hours, minutes, seconds] = parts;
            return new TimeSpan(hours, minutes, seconds);
        } else if (parts.length === 2) {
            const [minutes, seconds] = parts;
            return new TimeSpan(0, minutes, seconds);
        } else {
            throw new Error('Invalid TimeSpan format');
        }
    }

    get hours(): number {
        return Math.floor(this.totalMilliseconds / (60 * 60 * 1000));
    }

    get minutes(): number {
        return Math.floor((this.totalMilliseconds % (60 * 60 * 1000)) / (60 * 1000));
    }

    get seconds(): number {
        return Math.floor((this.totalMilliseconds % (60 * 1000)) / 1000);
    }

    get milliseconds(): number {
        return this.totalMilliseconds % 1000;
    }

    toString(): string {
        const hours = String(this.hours).padStart(2, '0');
        const minutes = String(this.minutes).padStart(2, '0');
        const seconds = String(this.seconds).padStart(2, '0');
        return `${hours}:${minutes}:${seconds}`;
    }
}