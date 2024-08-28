import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { SlotDataResponse } from "../../../shared";

@Injectable({
    providedIn: 'root'
})
export abstract class ServerSlotDataCollector {
    abstract getServerSlotData(slotKey: string): Observable<SlotDataResponse>;
}