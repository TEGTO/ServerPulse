import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { catchError, map, mergeMap, of } from "rxjs";
import { getAuthData, getAuthDataFailure, getAuthDataSuccess, logOutUser, logOutUserSuccess, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerFailure, registerSuccess, registerUser, signInUser, signInUserFailure, signInUserSuccess, updateUserData, updateUserDataFailure, updateUserDataSuccess } from "../..";
import { AuthData, AuthenticationApiService, getAuthDataFromAuthToken, getUserFromAuthResponse, getUserFromUpdateRequest, LocalStorageService, UserData } from "../../../shared";

//Registration
@Injectable()
export class RegistrationEffects {
    constructor(
        private readonly actions$: Actions,
        private readonly apiService: AuthenticationApiService
    ) { }

    registerUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(registerUser),
            mergeMap((action) =>
                this.apiService.registerUser(action.registrationRequest).pipe(
                    map(() => registerSuccess()),
                    catchError(error => of(registerFailure({ error: error.message })))
                )
            )
        )
    );
}
//Auth
@Injectable()
export class SignInEffects {
    readonly storageAuthDataKey: string = "authData";
    readonly storageUserDataKey: string = "userData";

    constructor(
        private readonly actions$: Actions,
        private readonly apiService: AuthenticationApiService,
        private readonly localStorage: LocalStorageService
    ) { }

    singInUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(signInUser),
            mergeMap((action) =>
                this.apiService.loginUser(action.authRequest).pipe(
                    map((response) => {
                        let authData: AuthData = getAuthDataFromAuthToken(response.authToken);
                        let userData: UserData = getUserFromAuthResponse(response);
                        this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(authData));
                        this.localStorage.setItem(this.storageUserDataKey, JSON.stringify(userData));
                        return signInUserSuccess({ authData: authData, userData: userData });
                    }),
                    catchError(error => of(signInUserFailure({ error: error.message })))
                )
            )
        )
    );
    getAuthUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getAuthData),
            mergeMap(() => {
                const jsonAuthData = this.localStorage.getItem(this.storageAuthDataKey);
                const jsonUserData = this.localStorage.getItem(this.storageUserDataKey);
                if (jsonAuthData !== null && jsonUserData !== null) {
                    const authData: AuthData = JSON.parse(jsonAuthData);
                    const userData: UserData = JSON.parse(jsonUserData);
                    return of(getAuthDataSuccess({ authData: authData, userData: userData }));
                }
                else {
                    return of(getAuthDataFailure());
                }
            })
        )
    );
    logOutUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(logOutUser),
            mergeMap(() => {
                this.localStorage.removeItem(this.storageAuthDataKey);
                this.localStorage.removeItem(this.storageUserDataKey);
                return of(logOutUserSuccess());
            })
        )
    );
    refreshToken$ = createEffect(() =>
        this.actions$.pipe(
            ofType(refreshAccessToken),
            mergeMap((action) =>
                this.apiService.refreshToken(action.authToken).pipe(
                    map((response) => {
                        let authData: AuthData = getAuthDataFromAuthToken(response);
                        this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(authData));
                        return refreshAccessTokenSuccess({ authData: response });
                    }),
                    catchError(error => {
                        this.localStorage.removeItem(this.storageAuthDataKey);
                        return of(refreshAccessTokenFailure({ error: error.message }));
                    })
                )
            )
        )
    );
    updateUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(updateUserData),
            mergeMap((action) =>
                this.apiService.updateUser(action.updateRequest).pipe(
                    map(() => {
                        let userData: UserData = getUserFromUpdateRequest(action.updateRequest);
                        this.localStorage.setItem(this.storageUserDataKey, JSON.stringify(userData));
                        return updateUserDataSuccess({ userData: userData });
                    }),
                    catchError(error => of(updateUserDataFailure({ error: error.message })))
                )
            )
        )
    );
}