import { Observable } from "rxjs";
import { LoadAmountStatisticsResponse, ServerLoadResponse, TimeSpan } from "../../../shared";

export abstract class ServerStatisticsService {
    abstract getStatisticsInDateRange(key: string, from: Date, to: Date): Observable<ServerLoadResponse[]>;
    abstract getAmountStatisticsInRange(key: string, from: Date, to: Date, timeSpan: TimeSpan): Observable<LoadAmountStatisticsResponse[]>;
    abstract getCurrentLoadStatisticsDate(): Observable<Date>;
    abstract setCurrentLoadStatisticsDate(date: Date): void;
}