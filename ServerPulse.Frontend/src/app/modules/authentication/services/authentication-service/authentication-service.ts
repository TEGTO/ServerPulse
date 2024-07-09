import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { AccessTokenDto, UserAuthData, UserAuthenticationRequest, UserRegistrationRequest, UserUpdateDataRequest } from "../../../shared";

@Injectable({
    providedIn: 'root'
})
export abstract class AuthenticationService {
    abstract registerUser(userRegistrationData: UserRegistrationRequest): Observable<boolean>;
    abstract registerUserGetErrors(): Observable<any>;
    abstract singInUser(userAuthData: UserAuthenticationRequest): Observable<UserAuthData>;
    abstract getAuthUserData(): Observable<UserAuthData>;
    abstract logOutUser(): Observable<UserAuthData>;
    abstract refreshToken(accessToken: AccessTokenDto): Observable<UserAuthData>;
    abstract updateUser(updateUserData: UserUpdateDataRequest): Observable<boolean>;
    abstract getAuthUserErrors(): Observable<any>;
}
