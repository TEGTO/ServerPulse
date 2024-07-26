import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { CreateServerSlotRequest, ServerSlot, UpdateServerSlotRequest } from "../../../shared";

@Injectable({
    providedIn: 'root'
})
export abstract class ServerSlotService {
    abstract getServerSlots(): Observable<ServerSlot[]>;
    abstract getServerSlotsWithString(str: string): Observable<ServerSlot[]>;
    abstract createServerSlot(request: CreateServerSlotRequest): void;
    abstract updateServerSlot(request: UpdateServerSlotRequest): void;
    abstract deleteServerSlot(id: string): void;
}