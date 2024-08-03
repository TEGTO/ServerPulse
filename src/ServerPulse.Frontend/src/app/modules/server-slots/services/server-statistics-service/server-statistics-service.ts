import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export abstract class ServerStatisticsService {

    abstract startConnection(): Observable<void>;
    abstract receiveStatistics(): Observable<{ key: string, data: string }>;
    abstract startListenPulse(key: string): void;
}
