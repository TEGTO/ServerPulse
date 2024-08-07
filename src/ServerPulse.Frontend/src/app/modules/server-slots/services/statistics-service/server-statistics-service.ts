import { Observable } from "rxjs";
import { ServerLoadResponse } from "../../../shared";

export abstract class ServerStatisticsService {
    abstract getStatisticsInDateRange(key: string, from: Date, to: Date): Observable<ServerLoadResponse[]>;
}
