import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export abstract class ServerStatisticsService {

    abstract startConnection(hubUrl: string): Observable<void>;
    abstract receiveStatistics(hubUrl: string): Observable<{ key: string, data: string }>;
    abstract startListen(hubUrl: string, key: string): void;
}
