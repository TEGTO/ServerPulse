import { Injectable } from "@angular/core";
import { MatDialogRef } from "@angular/material/dialog";

@Injectable({
    providedIn: 'root'
})
export abstract class ServerSlotDialogManager {
    abstract openDeleteSlotConfirmMenu(): MatDialogRef<any>;
}
