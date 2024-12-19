/* eslint-disable @typescript-eslint/no-explicit-any */
import { Injectable } from "@angular/core";
import { MatDialogRef } from "@angular/material/dialog";

@Injectable({
    providedIn: 'root'
})
export abstract class AuthenticationDialogManager {
    abstract closeAll(): void;
    abstract openLoginMenu(): MatDialogRef<any>;
    abstract openRegisterMenu(): MatDialogRef<any>;
    abstract openAuthenticatedMenu(): MatDialogRef<any>;
}
