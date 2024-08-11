import { Observable } from "rxjs";
import { LoadAmountStatisticsResponse, ServerLoadResponse, ServerLoadStatisticsResponse, ServerStatisticsResponse, TimeSpan } from "../../../shared";

export abstract class ServerStatisticsService {
    abstract getWholeAmountStatisticsInDays(key: string): Observable<LoadAmountStatisticsResponse[]>;
    abstract getStatisticsInDateRange(key: string, from: Date, to: Date): Observable<ServerLoadResponse[]>;
    abstract getAmountStatisticsInRange(key: string, from: Date, to: Date, timeSpan: TimeSpan): Observable<LoadAmountStatisticsResponse[]>;
    abstract getLastServerStatistics(key: string): Observable<{ key: string; statistics: ServerStatisticsResponse; } | null>;
    abstract getLastServerLoadStatistics(key: string): Observable<{ key: string; statistics: ServerLoadStatisticsResponse; } | null>;
    abstract getCurrentLoadStatisticsDate(): Observable<Date>;
    abstract setCurrentLoadStatisticsDate(date: Date): void;
}