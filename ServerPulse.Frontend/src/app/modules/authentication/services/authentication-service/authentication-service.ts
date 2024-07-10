import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { AuthData, AuthToken, UserAuthenticationRequest, UserData, UserRegistrationRequest, UserUpdateDataRequest } from "../../../shared";

@Injectable({
    providedIn: 'root'
})
export abstract class AuthenticationService {
    abstract registerUser(userRegistrationData: UserRegistrationRequest): Observable<boolean>;
    abstract registerUserGetErrors(): Observable<any>;
    abstract singInUser(userAuthData: UserAuthenticationRequest): Observable<AuthData>;
    abstract getAuthData(): Observable<AuthData>;
    abstract getUserData(): Observable<UserData>;
    abstract logOutUser(): Observable<AuthData>;
    abstract refreshToken(accessToken: AuthToken): Observable<AuthData>;
    abstract updateUser(updateUserData: UserUpdateDataRequest): Observable<boolean>;
    abstract getAuthErrors(): Observable<any>;
}
