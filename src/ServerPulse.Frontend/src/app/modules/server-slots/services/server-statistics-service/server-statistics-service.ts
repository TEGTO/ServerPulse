import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ServerStatisticsResponse } from "../../../shared";

@Injectable({
    providedIn: 'root'
})
export abstract class ServerStatisticsService {

    abstract getCurrentServerStatisticsByKey(key: string): Observable<ServerStatisticsResponse>;
}
