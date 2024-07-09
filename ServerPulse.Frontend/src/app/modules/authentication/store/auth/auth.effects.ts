import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { catchError, map, mergeMap, of } from "rxjs";
import { getAuthUserData, getAuthUserDataSuccess, logOutUser, logOutUserSuccess, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerFailure, registerSuccess, registerUser, signInUser, signInUserFailure, signInUserSuccess, updateUserData, updateUserDataFailure, updateUserDataSuccess } from "../..";
import { AuthenticationApiService, LocalStorageService, UserAuthData } from "../../../shared";

//Registration
@Injectable()
export class RegistrationEffects {
    constructor(private actions$: Actions,
        private apiService: AuthenticationApiService) { }

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
    readonly storageUserAuthDataKey: string = "UserAuthData";

    constructor(private actions$: Actions,
        private apiService: AuthenticationApiService, private localStorage: LocalStorageService) { }

    singInUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(signInUser),
            mergeMap((action) =>
                this.apiService.loginUser(action.userAuthData).pipe(
                    map((response) => {
                        let userData: UserAuthData = {
                            isAuthenticated: true,
                            authToken: response.accessToken,
                            refreshToken: response.refreshToken,
                            refreshTokenExpiryDate: response.refreshTokenExpiryDate,
                            userEmail: action.userAuthData.email
                        }
                        this.localStorage.setItem(this.storageUserAuthDataKey, JSON.stringify(userData));
                        return signInUserSuccess({ userAuthData: userData });
                    }),
                    catchError(error => of(signInUserFailure({ error: error.message })))
                )
            )
        )
    );
    getAuthUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getAuthUserData),
            mergeMap(() => {
                const json = this.localStorage.getItem(this.storageUserAuthDataKey);
                if (json !== null) {
                    const userData = JSON.parse(json) as UserAuthData;
                    return of(getAuthUserDataSuccess({ userAuthData: userData }));
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
                const json = this.localStorage.getItem(this.storageUserAuthDataKey);
                if (json !== null) {
                    this.localStorage.removeItem(this.storageUserAuthDataKey);
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
                        let json = this.localStorage.getItem(this.storageUserAuthDataKey);
                        if (json !== null) {
                            let userData = JSON.parse(json) as UserAuthData;
                            userData.authToken = response.accessToken;
                            this.localStorage.setItem(this.storageUserAuthDataKey, JSON.stringify(userData));
                        }
                        return refreshAccessTokenSuccess({ accessToken: response });
                    }),
                    catchError(error => {
                        this.localStorage.removeItem(this.storageUserAuthDataKey);
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
                        const json = this.localStorage.getItem(this.storageUserAuthDataKey);
                        if (json !== null) {
                            let userData = JSON.parse(json) as UserAuthData;
                            userData.userEmail = action.userUpdateData.newEmail
                                ? action.userUpdateData.newEmail
                                : action.userUpdateData.oldEmail;
                            this.localStorage.setItem(this.storageUserAuthDataKey, JSON.stringify(userData));
                        }
                        return updateUserDataSuccess({ userUpdateData: action.userUpdateData });
                    }),
                    catchError(error => of(updateUserDataFailure({ error: error.message })))
                )
            )
        )
    );
}