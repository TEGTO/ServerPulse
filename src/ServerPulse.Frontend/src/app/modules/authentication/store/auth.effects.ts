/* eslint-disable @typescript-eslint/no-unused-vars */
import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { Store } from "@ngrx/store";
import { catchError, map, mergeMap, of, switchMap, withLatestFrom } from "rxjs";
import { AuthData, AuthenticationApiService, AuthenticationDialogManagerService, copyAuthTokenToAuthData, copyUserUpdateRequestToUserAuth, getAuthData, getAuthDataFailure, getAuthDataSuccess, loginUser, loginUserFailure, loginUserSuccess, logOutUser, logOutUserSuccess, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerFailure, registerSuccess, registerUser, selectAuthState, startLoginUser, startRegisterUser, updateUserData, updateUserDataFailure, updateUserDataSuccess } from "..";
import { LocalStorageService, RedirectorService, SnackbarManager } from "../../shared";

@Injectable({
    providedIn: 'root'
})
export class AuthEffects {
    readonly storageAuthDataKey: string = "authData";

    constructor(
        private readonly actions$: Actions,
        private readonly store: Store,
        private readonly authApiService: AuthenticationApiService,
        private readonly localStorage: LocalStorageService,
        private readonly redirector: RedirectorService,
        private readonly snackbarManager: SnackbarManager,
        private readonly dialogManager: AuthenticationDialogManagerService
    ) { }

    startRegisterUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(startRegisterUser),
            switchMap(() => {
                this.dialogManager.openRegisterMenu();
                return of();
            })
        ),
        { dispatch: false }
    );

    registerUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(registerUser),
            mergeMap((action) =>
                this.authApiService.registerUser(action.req).pipe(
                    map((response) => {
                        this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(response));

                        this.snackbarManager.openInfoSnackbar('✔️ The registration is successful!', 5);
                        this.dialogManager.closeAll();
                        this.redirector.redirectToHome();

                        return registerSuccess({ authData: response });
                    }),
                    catchError(error => of(registerFailure({ error: error.message })))
                )
            )
        )
    );

    startLoginUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(startLoginUser),
            withLatestFrom(this.store.select(selectAuthState)),
            switchMap(([action, authState]) => {
                if (authState.authData.isAuthenticated) {
                    this.dialogManager.openAuthenticatedMenu();
                }
                else {
                    this.dialogManager.openLoginMenu();
                }
                return of();
            })
        ),
        { dispatch: false }
    );

    loginUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(loginUser),
            mergeMap((action) =>
                this.authApiService.loginUser(action.req).pipe(
                    map((response) => {
                        this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(response));

                        this.dialogManager.closeAll();
                        this.redirector.redirectToHome();

                        return loginUserSuccess({ authData: response });
                    }),
                    catchError(error => of(loginUserFailure({ error: error.message })))
                )
            )
        )
    );

    getAuthData$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getAuthData),
            mergeMap(() => {
                const json = this.localStorage.getItem(this.storageAuthDataKey);
                if (json !== null) {
                    const authData: AuthData = JSON.parse(json);
                    return of(getAuthDataSuccess({ authData: authData }));
                }
                else {
                    return of(getAuthDataFailure());
                }
            }),
            catchError(() => of(getAuthDataFailure()))
        )
    );

    logOutUser$ = createEffect(() =>
        this.actions$.pipe(
            ofType(logOutUser),
            mergeMap(() => {
                this.localStorage.removeItem(this.storageAuthDataKey);
                this.redirector.redirectToHome();
                return of(logOutUserSuccess());
            })
        )
    );

    refreshToken$ = createEffect(() =>
        this.actions$.pipe(
            ofType(refreshAccessToken),
            mergeMap((action) =>
                this.authApiService.refreshToken(action.authToken).pipe(
                    map((response) => {
                        const json = this.localStorage.getItem(this.storageAuthDataKey);
                        let authData: AuthData = JSON.parse(json!);
                        authData = copyAuthTokenToAuthData(authData, response);
                        this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(authData));
                        return refreshAccessTokenSuccess({ authToken: response });
                    }),
                    catchError(error => {
                        this.localStorage.removeItem(this.storageAuthDataKey);
                        return of(refreshAccessTokenFailure({ error: error.message }));
                    })
                )
            )
        )
    );
    refreshAccessTokenFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(refreshAccessTokenFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Token refresh failed: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    updateUserData$ = createEffect(() =>
        this.actions$.pipe(
            ofType(updateUserData),
            mergeMap((action) =>
                this.authApiService.updateUser(action.req).pipe(
                    map(() => {
                        const json = this.localStorage.getItem(this.storageAuthDataKey);
                        let authData: AuthData = JSON.parse(json!);
                        authData = copyUserUpdateRequestToUserAuth(authData, action.req);
                        this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(authData));

                        this.snackbarManager.openInfoSnackbar('✔️ The update is successful!', 5);
                        this.dialogManager.closeAll();

                        return updateUserDataSuccess({ req: action.req });
                    }),
                    catchError(error => {
                        return of(updateUserDataFailure({ error: error.message }));
                    })
                )
            )
        )
    );
}