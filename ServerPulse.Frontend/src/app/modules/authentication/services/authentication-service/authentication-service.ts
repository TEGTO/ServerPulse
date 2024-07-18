import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { AuthData, AuthToken, UserAuthenticationRequest, UserData, UserRegistrationRequest, UserUpdateDataRequest } from "../../../shared";

@Injectable({
    providedIn: 'root'
})
export abstract class AuthenticationService {
    //Registration
    abstract registerUser(userRegistrationData: UserRegistrationRequest): Observable<boolean>;
    abstract getRegistrationErrors(): Observable<any>;
    //Auth
    abstract singInUser(authRequest: UserAuthenticationRequest): Observable<AuthData>;
    abstract getAuthData(): Observable<AuthData>;
    abstract getAuthErrors(): Observable<any>;
    abstract logOutUser(): Observable<AuthData>;
    abstract refreshToken(authToken: AuthToken): Observable<AuthData>;
    //User
    abstract getUserData(): Observable<UserData>;
    abstract updateUser(updateRquest: UserUpdateDataRequest): Observable<boolean>;
    abstract getUserErrors(): Observable<any>;
}
