/* eslint-disable @typescript-eslint/no-unused-vars */
import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { Store } from "@ngrx/store";
import { catchError, map, of, switchMap, withLatestFrom } from "rxjs";
import { AuthData, AuthenticationApiService, AuthenticationDialogManagerService, authFailure, confirmEmail, copyAccessTokenDataToAuthData, copyUserUpdateRequestToUserAuth, getAuthData, getAuthDataFailure, getAuthDataSuccess, getFullOAuthRedirectPath, GetOAuthUrlParams, LoginOAuthRequest, loginUser, loginUserSuccess, logOutUser, logOutUserSuccess, OauthApiService, oauthLogin, oauthLoginFailure, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerUser, registerUserSuccess, selectAuthState, startLoginUser, startOAuthLogin, startOAuthLoginFailure, startRegisterUser, updateUserData, updateUserDataSuccess } from "..";
import { environment } from "../../../../environment/environment";
import { LocalStorageService, RedirectorService, SnackbarManager } from "../../shared";

@Injectable({
    providedIn: 'root'
})
export class AuthEffects {
    private readonly storageAuthDataKey: string = "authData";
    private readonly storageOAuthParamsKey: string = "OAuthParams";

    constructor(
        private readonly actions$: Actions,
        private readonly store: Store,
        private readonly authApiService: AuthenticationApiService,
        private readonly oAuthApiService: OauthApiService,
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
            switchMap((action) =>
                this.authApiService.registerUser(action.req).pipe(
                    map(() => {
                        if (!environment.isConfirmEmailEnabled) {
                            return loginUser({ req: { login: action.req.email, password: action.req.password } });
                        }
                        return registerUserSuccess();
                    }),
                    catchError((error) => of(authFailure({ error: error.message })))
                )
            )
        )
    );
    registerUserSuccess$ = createEffect(() =>
        this.actions$.pipe(
            ofType(registerUserSuccess),
            switchMap(() => {
                this.snackbarManager.openInfoSnackbar('✔️ The registration is successful! Please confirm the email!', 15);
                this.dialogManager.closeAll();
                this.redirector.redirectToHome();
                return of();
            })
        ),
        { dispatch: false }
    );

    confirmEmail$ = createEffect(() =>
        this.actions$.pipe(
            ofType(confirmEmail),
            switchMap((action) =>
                this.authApiService.confirmEmail(action.req).pipe(
                    map((response) => {
                        this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(response));

                        this.dialogManager.closeAll();
                        this.redirector.redirectToHome();

                        return loginUserSuccess({ authData: response });
                    }),
                    catchError(error => of(authFailure({ error: error.message })))
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
            switchMap((action) =>
                this.authApiService.loginUser(action.req).pipe(
                    map((response) => {
                        this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(response));

                        this.dialogManager.closeAll();
                        this.redirector.redirectToHome();

                        return loginUserSuccess({ authData: response });
                    }),
                    catchError(error => of(authFailure({ error: error.message })))
                )
            )
        )
    );

    getAuthData$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getAuthData),
            switchMap(() => {
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
            switchMap(() => {
                this.localStorage.removeItem(this.storageAuthDataKey);
                this.redirector.redirectToHome();
                return of(logOutUserSuccess());
            })
        )
    );

    refreshToken$ = createEffect(() =>
        this.actions$.pipe(
            ofType(refreshAccessToken),
            switchMap((action) =>
                this.authApiService.refreshToken(action.accessTokenData).pipe(
                    map((response) => {
                        const json = this.localStorage.getItem(this.storageAuthDataKey);
                        let authData: AuthData = JSON.parse(json!);
                        authData = copyAccessTokenDataToAuthData(authData, response);
                        this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(authData));
                        return refreshAccessTokenSuccess({ accessTokenData: response });
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
            switchMap((action) =>
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
                        return of(authFailure({ error: error.message }));
                    })
                )
            )
        )
    );

    oauthLogin$ = createEffect(() =>
        this.actions$.pipe(
            ofType(oauthLogin),
            switchMap((action) => {
                const json = this.localStorage.getItem(this.storageOAuthParamsKey);
                if (json !== null) {
                    const params: GetOAuthUrlParams = JSON.parse(json);

                    const req: LoginOAuthRequest = {
                        queryParams: action.queryParams,
                        redirectUrl: params.redirectUrl,
                        oAuthLoginProvider: params.oAuthLoginProvider
                    };

                    return this.oAuthApiService.loginUserOAuth(req).pipe(
                        map((response) => {
                            this.localStorage.setItem(this.storageAuthDataKey, JSON.stringify(response));
                            return loginUserSuccess({ authData: response });
                        }),
                        catchError(error => of(authFailure({ error: error.message })))
                    )
                }
                return of(oauthLoginFailure({ error: new Error("Failed to get oauth url params!") }));
            })
        )
    );
    oauthLoginFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(oauthLoginFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["OAuth login failed: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    startOAuthLogin$ = createEffect(() =>
        this.actions$.pipe(
            ofType(startOAuthLogin),
            switchMap((action) => {

                const req: GetOAuthUrlParams = {
                    redirectUrl: getFullOAuthRedirectPath(),
                    oAuthLoginProvider: action.loginProvider
                };

                this.localStorage.setItem(this.storageOAuthParamsKey, JSON.stringify(req));

                return this.oAuthApiService.getOAuthUrl(req).pipe(
                    map((response) => {
                        this.redirector.redirectToExternalUrl(response.url);
                        return of();
                    }),
                    catchError(error => of(startOAuthLoginFailure({ error })))
                )
            })
        ),
        { dispatch: false }
    );
    startOAuthLoginFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(startOAuthLoginFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to get oauth url: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );
}