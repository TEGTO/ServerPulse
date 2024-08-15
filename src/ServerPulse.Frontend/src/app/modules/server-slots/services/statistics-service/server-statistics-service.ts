import { Observable } from "rxjs";
import { LoadAmountStatisticsResponse, ServerLoadResponse, ServerLoadStatisticsResponse, ServerStatisticsResponse, TimeSpan } from "../../../shared";

export abstract class ServerStatisticsService {
    abstract getWholeAmountStatisticsInDays(key: string): Observable<LoadAmountStatisticsResponse[]>;
    abstract getLoadEventsInDateRange(key: string, from: Date, to: Date): Observable<ServerLoadResponse[]>;
    abstract getSomeLoadEventsFromDate(key: string, numberOfMessages: number, from: Date, readNew: boolean): Observable<ServerLoadResponse[]>;
    abstract getAmountStatisticsInRange(key: string, from: Date, to: Date, timeSpan: TimeSpan): Observable<LoadAmountStatisticsResponse[]>;
    abstract getLastServerStatistics(key: string): Observable<{ key: string; statistics: ServerStatisticsResponse; } | null>;
    abstract getLastServerLoadStatistics(key: string): Observable<{ key: string; statistics: ServerLoadStatisticsResponse; } | null>;
    abstract getCurrentLoadStatisticsDate(): Observable<Date>;
    abstract setCurrentLoadStatisticsDate(date: Date): void;
}