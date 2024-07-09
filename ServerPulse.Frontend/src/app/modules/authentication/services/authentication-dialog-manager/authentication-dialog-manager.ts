import { Injectable } from "@angular/core";
import { MatDialogRef } from "@angular/material/dialog";

@Injectable({
    providedIn: 'root'
})
export abstract class AuthenticationDialogManager {
    abstract openLoginMenu(): MatDialogRef<any>;
    abstract openRegisterMenu(): MatDialogRef<any>;
}
