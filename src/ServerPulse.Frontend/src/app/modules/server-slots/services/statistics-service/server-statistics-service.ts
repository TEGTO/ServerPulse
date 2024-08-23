import { Observable } from "rxjs";
import { CustomEventResponse, CustomEventStatisticsResponse, LoadAmountStatisticsResponse, ServerLoadResponse, ServerLoadStatisticsResponse, ServerStatisticsResponse, TimeSpan } from "../../../shared";

export abstract class ServerStatisticsService {
    abstract getWholeLoadAmountStatisticsInDays(key: string): Observable<LoadAmountStatisticsResponse[]>;
    abstract getLoadEventsInDateRange(key: string, from: Date, to: Date): Observable<ServerLoadResponse[]>;
    abstract getSomeLoadEventsFromDate(key: string, numberOfMessages: number, from: Date, readNew: boolean): Observable<ServerLoadResponse[]>;
    abstract getSomeCustomEventsFromDate(key: string, numberOfMessages: number, from: Date, readNew: boolean): Observable<CustomEventResponse[]>;
    abstract getLoadAmountStatisticsInRange(key: string, from: Date, to: Date, timeSpan: TimeSpan): Observable<LoadAmountStatisticsResponse[]>;
    abstract getLastServerStatistics(key: string): Observable<{ key: string; statistics: ServerStatisticsResponse; } | null>;
    abstract getLastServerLoadStatistics(key: string): Observable<{ key: string; statistics: ServerLoadStatisticsResponse; } | null>;
    abstract getLastCustomStatistics(key: string): Observable<{ key: string; statistics: CustomEventStatisticsResponse; } | null>;
    abstract getCurrentLoadStatisticsDate(): Observable<Date>;
    abstract setCurrentLoadStatisticsDate(date: Date): void;
}