export function isSelectedDateToday(date: Date): boolean {
    const checkDate = new Date(date);
    return checkDate.setHours(0, 0, 0, 0) >= new Date().setHours(0, 0, 0, 0);
}