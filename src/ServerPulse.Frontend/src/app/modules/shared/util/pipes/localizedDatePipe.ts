import { Inject, LOCALE_ID, Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'localizedDate'
})
export class LocalizedDatePipe implements PipeTransform {

    constructor(@Inject(LOCALE_ID) private readonly locale: string) { }

    transform(value: Date | string | number | null | undefined): string | null {
        if (value === null || value === undefined) {
            return null;
        }
        const date = new Date(value);
        if (isNaN(date.getTime())) {
            return null;
        }
        return date.toLocaleDateString(undefined, {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: 'numeric',
            minute: 'numeric'
        });
    }
}
