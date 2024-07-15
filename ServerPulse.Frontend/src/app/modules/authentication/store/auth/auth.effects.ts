import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { catchError, map, mergeMap, of } from "rxjs";
import { getAuthData, getAuthDataSuccess, logOutUser, logOutUserSuccess, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerFailure, registerSuccess, registerUser, signInUser, signInUserFailure, signInUserSuccess, updateUserData, updateUserDataFailure, updateUserDataSuccess } from "../..";
import { AuthData, AuthenticationApiService, LocalStorageService, UserData } from "../../../shared";

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
                this.apiService.registerUser(action.userRegistrationData).pipe(
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
                this.apiService.loginUser(action.authData).pipe(
                    map((response) => {
                        let authData: AuthData = {
                            isAuthenticated: true,
                            authToken: response.authToken.accessToken,
                            refreshToken: response.authToken.refreshToken,
                            refreshTokenExpiryDate: response.authToken.refreshTokenExpiryDate
                        };
                        let userData: UserData =
                        {
                            userName: response.userName,
                            email: response.email,
                        }
                        this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(authData));
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
                    const authData = JSON.parse(jsonAuthData) as AuthData;
                    const userData = JSON.parse(jsonUserData) as UserData;
                    return of(getAuthDataSuccess({ authData: authData, userData: userData }));
                }
                else {
                    return of();
                }
            })
        )
    );
    logOutUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(logOutUser),
            mergeMap(() => {
                let json = this.localStorage.getItem(this.storageAuthDataKey);
                if (json !== null) {
                    this.localStorage.removeItem(this.storageAuthDataKey);
                }
                json = this.localStorage.getItem(this.storageUserDataKey);
                if (json !== null) {
                    this.localStorage.removeItem(this.storageUserDataKey);
                }
                return of(logOutUserSuccess());
            })
        )
    );
    refreshToken$ = createEffect(() =>
        this.actions$.pipe(
            ofType(refreshAccessToken),
            mergeMap((action) =>
                this.apiService.refreshToken(action.accessToken).pipe(
                    map((response) => {
                        let json = this.localStorage.getItem(this.storageAuthDataKey);
                        if (json !== null) {
                            let authData = JSON.parse(json) as AuthData;
                            authData.authToken = response.accessToken;
                            this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(authData));
                        }
                        return refreshAccessTokenSuccess({ accessToken: response });
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
                this.apiService.updateUser(action.userUpdateData).pipe(
                    map(() => {
                        let userData: UserData =
                        {
                            userName: action.userUpdateData.userName,
                            email: action.userUpdateData.newEmail
                                ? action.userUpdateData.newEmail
                                : action.userUpdateData.oldEmail
                        };
                        this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(userData));
                        return updateUserDataSuccess({ userData: userData });
                    }),
                    catchError(error => of(updateUserDataFailure({ error: error.message })))
                )
            )
        )
    );
}