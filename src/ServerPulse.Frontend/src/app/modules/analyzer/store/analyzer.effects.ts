import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { Store } from "@ngrx/store";
import { catchError, filter, map, mergeMap, of, switchMap, tap, withLatestFrom } from "rxjs";
import { AnalyzerApiService, getLoadAmountStatisticsInRange, getLoadAmountStatisticsInRangeFailure, getLoadAmountStatisticsInRangeSuccess, mapServerCustomStatisticsResponseToServerCustomStatistics, mapServerLifecycleStatisticsResponseToServerLifecycleStatistics, mapServerLoadStatisticsResponseToServerLoadStatistics, receiveCustomStatisticsFailure, receiveCustomStatisticsSuccess, receiveLifecycleStatisticsFailure, receiveLifecycleStatisticsSuccess, receiveLoadStatisticsFailure, receiveLoadStatisticsSuccess, ServerCustomStatisticsResponse, ServerLifecycleStatisticsResponse, ServerLoadStatisticsResponse, SignalStatisticsService, startCustomStatisticsReceiving, startLifecycleStatisticsReceiving, startLoadStatisticsReceiving, stopCustomStatisticsReceiving, stopLifecycleStatisticsReceiving, stopLoadStatisticsReceiving } from "..";
import { environment } from "../../../../environment/environment";
import { selectAuthData } from "../../authentication";
import { SnackbarManager, TimeSpan } from "../../shared";

@Injectable({
    providedIn: 'root'
})
export class AnalyzerEffects {
    private readonly activeLifecycleStatisticsListeners = new Set<string>();
    private readonly activeLoadStatisticsListeners = new Set<string>();
    private readonly activeCustomStatisticsListeners = new Set<string>();

    constructor(
        private readonly actions$: Actions,
        private readonly apiService: AnalyzerApiService,
        private readonly store: Store,
        private readonly snackbarManager: SnackbarManager,
        private readonly signalStatistics: SignalStatisticsService,
    ) { }

    //#region Lifecycle Statistics

    startLifecycleStatisticsReceiving$ = createEffect(() =>
        this.actions$.pipe(
            ofType(startLifecycleStatisticsReceiving),
            withLatestFrom(this.store.select(selectAuthData)),
            filter(([action]) => {
                if (this.activeLifecycleStatisticsListeners.has(action.key)) {
                    console.warn(`Listener for the key in lifecycle statistics hub already exists. Skipping duplicate.`);
                    return false;
                }
                return true;
            }),
            switchMap(([action, authData]) => {
                const hubUrl = environment.lifecycleStatisticsHub;

                return this.signalStatistics.startConnection(hubUrl, authData.authToken.accessToken ?? "").pipe(
                    switchMap(() => {
                        this.signalStatistics.startListen(hubUrl, action.key);

                        const receiveResponse = this.signalStatistics
                            .receiveStatistics<ServerLifecycleStatisticsResponse>(hubUrl)
                            .pipe(
                                map((message) => {
                                    const statistics = mapServerLifecycleStatisticsResponseToServerLifecycleStatistics(
                                        message.response
                                    );
                                    return receiveLifecycleStatisticsSuccess({ key: message.key, statistics });
                                }),
                                catchError((error) =>
                                    of(receiveLifecycleStatisticsFailure({ error: error.message }))
                                )
                            );

                        this.activeLifecycleStatisticsListeners.add(action.key);

                        return receiveResponse;
                    }),
                    catchError((error) => {
                        return of(receiveLifecycleStatisticsFailure({ error: error.message }));
                    })
                );
            })
        )
    );
    receiveLifecycleStatisticsFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(receiveLifecycleStatisticsFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to receive statistics in lifecycle statistics hub: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    stopLifecycleStatisticsReceiving$ = createEffect(() =>
        this.actions$.pipe(
            ofType(stopLifecycleStatisticsReceiving),
            switchMap((action) => {
                this.signalStatistics.stopListen(environment.lifecycleStatisticsHub, action.key);
                this.activeLifecycleStatisticsListeners.delete(action.key);

                return of();
            })
        ),
        { dispatch: false }
    );

    //#endregion

    //#region Load Statistics

    startLoadStatisticsReceiving$ = createEffect(() =>
        this.actions$.pipe(
            ofType(startLoadStatisticsReceiving),
            withLatestFrom(this.store.select(selectAuthData)),
            filter(([action]) => {
                if (this.activeLoadStatisticsListeners.has(action.key)) {
                    console.warn(`Listener for the key in load statistics hub already exists. Skipping duplicate.`);
                    return false;
                }
                return true;
            }),
            mergeMap(([action, authData]) => {
                const hubUrl = environment.loadStatisticsHub;

                return this.signalStatistics.startConnection(hubUrl, authData.authToken.accessToken ?? "").pipe(
                    tap(() => {
                        this.signalStatistics.startListen(hubUrl, action.key);
                        this.activeLoadStatisticsListeners.add(action.key);
                    }),
                    mergeMap(() =>
                        this.signalStatistics.receiveStatistics<ServerLoadStatisticsResponse>(hubUrl).pipe(
                            map((message) => {
                                const statistics = mapServerLoadStatisticsResponseToServerLoadStatistics(message.response);
                                return receiveLoadStatisticsSuccess({ key: message.key, statistics });
                            }),
                            catchError((error) =>
                                of(receiveLoadStatisticsFailure({ error: error.message }))
                            )
                        )
                    ),
                    catchError((error) => of(receiveLoadStatisticsFailure({ error: error.message })))
                );
            }, 3)
        )
    );

    receiveLoadStatisticsFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(receiveLoadStatisticsFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to receive statistics in load statistics hub: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    stopLoadStatisticsReceiving$ = createEffect(() =>
        this.actions$.pipe(
            ofType(stopLoadStatisticsReceiving),
            switchMap((action) => {
                this.signalStatistics.stopListen(environment.loadStatisticsHub, action.key);
                this.activeLoadStatisticsListeners.delete(action.key);

                return of();
            })
        ),
        { dispatch: false }
    );

    //#endregion

    //#region Custom Statistics

    startCustomStatisticsReceiving$ = createEffect(() =>
        this.actions$.pipe(
            ofType(startCustomStatisticsReceiving),
            withLatestFrom(this.store.select(selectAuthData)),
            filter(([action]) => {
                if (this.activeCustomStatisticsListeners.has(action.key)) {
                    console.warn(`Listener for the key in custom statistics hub already exists. Skipping duplicate.`);
                    return false;
                }

                return true;
            }),
            switchMap(([action, authData]) => {
                const hubUrl = environment.customStatisticsHub;

                return this.signalStatistics.startConnection(hubUrl, authData.authToken.accessToken ?? "").pipe(
                    switchMap(() => {
                        this.signalStatistics.startListen(hubUrl, action.key);

                        const receiveResponse = this.signalStatistics
                            .receiveStatistics<ServerCustomStatisticsResponse>(hubUrl)
                            .pipe(
                                map((message) => {
                                    const statistics = mapServerCustomStatisticsResponseToServerCustomStatistics(
                                        message.response
                                    );
                                    return receiveCustomStatisticsSuccess({ key: message.key, statistics });
                                }),
                                catchError((error) =>
                                    of(receiveCustomStatisticsFailure({ error: error.message }))
                                )
                            );

                        this.activeCustomStatisticsListeners.add(action.key);

                        return receiveResponse;
                    }),
                    catchError((error) => {
                        return of(receiveCustomStatisticsFailure({ error: error.message }));
                    })
                );
            })
        )
    );
    receiveCustomStatisticsFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(receiveCustomStatisticsFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to receive statistics in custom statistics hub: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    stopCustomStatisticsReceiving$ = createEffect(() =>
        this.actions$.pipe(
            ofType(stopCustomStatisticsReceiving),
            switchMap((action) => {
                this.signalStatistics.stopListen(environment.customStatisticsHub, action.key);
                this.activeCustomStatisticsListeners.delete(action.key);

                return of();
            })
        ),
        { dispatch: false }
    );

    //#endregion

    //#region Loaf Amount Statistics

    getLoadAmountStatisticsInRange$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getLoadAmountStatisticsInRange),
            mergeMap((action) =>
                this.apiService.getLoadAmountStatisticsInRange(action.req).pipe(
                    map((response) =>
                        getLoadAmountStatisticsInRangeSuccess({
                            key: action.req.key,
                            statistics: response,
                            timespan: TimeSpan.fromString(action.req.timeSpan)
                        })
                    ),
                    catchError((error) =>
                        of(getLoadAmountStatisticsInRangeFailure({ error: error.message }))
                    )
                )
            )
        )
    );
    getLoadAmountStatisticsInRangeFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getLoadAmountStatisticsInRangeFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to receive load amount statistics in range: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    //#endregion
}