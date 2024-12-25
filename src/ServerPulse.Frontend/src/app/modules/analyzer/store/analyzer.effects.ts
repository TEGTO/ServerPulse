/* eslint-disable @typescript-eslint/no-explicit-any */
import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { Store } from "@ngrx/store";
import { catchError, concatMap, filter, map, mergeMap, of, switchMap, withLatestFrom } from "rxjs";
import { AnalyzerApiService, BaseStatisticsResponse, downloadSlotStatistics, downLoadSlotStatisticsFailure, getLoadAmountStatisticsInRange, getLoadAmountStatisticsInRangeFailure, getLoadAmountStatisticsInRangeSuccess, mapServerCustomStatisticsResponseToServerCustomStatistics, mapServerLifecycleStatisticsResponseToServerLifecycleStatistics, mapServerLoadStatisticsResponseToServerLoadStatistics, receiveCustomStatisticsFailure, receiveCustomStatisticsSuccess, receiveLifecycleStatisticsFailure, receiveLifecycleStatisticsSuccess, receiveLoadStatisticsFailure, receiveLoadStatisticsSuccess, ServerCustomStatisticsResponse, ServerLifecycleStatisticsResponse, ServerLoadStatisticsResponse, SignalStatisticsService, startCustomStatisticsReceiving, startLifecycleStatisticsReceiving, startLoadStatisticsReceiving, stopCustomStatisticsReceiving, stopLifecycleStatisticsReceiving, stopLoadKeyListening } from "..";
import { environment } from "../../../../environment/environment";
import { selectAuthData } from "../../authentication";
import { JsonDownloader, SnackbarManager, TimeSpan } from "../../shared";

@Injectable({
    providedIn: 'root'
})
export class AnalyzerEffects {
    private readonly activeListeners = {
        lifecycle: new Set<string>(),
        load: new Set<string>(),
        custom: new Set<string>(),
    };

    get maxAmountOfSimultaneousConnections(): number {
        return environment.maxAmountOfSlotsPerUser;
    }

    constructor(
        private readonly actions$: Actions,
        private readonly apiService: AnalyzerApiService,
        private readonly store: Store,
        private readonly snackbarManager: SnackbarManager,
        private readonly signalStatistics: SignalStatisticsService,
        private readonly jsonDownloader: JsonDownloader
    ) { }


    //#region Generic Helpers

    private handleStatisticsReceiving<T extends BaseStatisticsResponse>(
        hubUrl: string,
        actionType: any,
        successAction: any,
        failureAction: any,
        activeListeners: Set<string>,
        statisticsMapper: (response: T) => any
    ) {
        return this.actions$.pipe(
            ofType(actionType),
            withLatestFrom(this.store.select(selectAuthData)),
            filter(([action]) => {
                if (activeListeners.has(action.key)) {
                    console.warn(`Listener for the key in hub already exists. Skipping duplicate.`);
                    return false;
                }
                return true;
            }),
            mergeMap(([action, authData]) =>
                this.signalStatistics.startConnection(hubUrl, authData.authToken.accessToken ?? "").pipe(
                    mergeMap(() => {
                        this.signalStatistics.startListen(hubUrl, action.key, action.getInitial ?? true);
                        activeListeners.add(action.key);

                        return this.signalStatistics.receiveStatistics<T>(hubUrl).pipe(
                            map((message) =>
                                successAction({
                                    key: message.key,
                                    statistics: statisticsMapper(message.response),
                                })
                            ),
                            catchError((error) =>
                                of(failureAction({ error: error.message }))
                            )
                        );
                    }),
                    catchError((error) =>
                        of(failureAction({ error: error.message }))
                    )
                )
            )
        );
    }

    private handleFailure(failureAction: any, errorMessage: string) {
        return this.actions$.pipe(
            ofType(failureAction),
            switchMap((action: any) => {
                this.snackbarManager.openErrorSnackbar([`${errorMessage}: ${action.error}`]);
                return of();
            })
        );
    }

    private handleStopListening(hubUrl: string, actionType: any, activeListeners: Set<string>) {
        return this.actions$.pipe(
            ofType(actionType),
            switchMap((action: any) => {
                this.signalStatistics.stopListen(hubUrl, action.key);
                activeListeners.delete(action.key);
                return of();
            })
        );
    }

    //#endregion

    //#region Lifecycle Statistics

    startLifecycleStatisticsReceiving$ = createEffect(() =>
        this.handleStatisticsReceiving<ServerLifecycleStatisticsResponse>(
            environment.lifecycleStatisticsHub,
            startLifecycleStatisticsReceiving,
            receiveLifecycleStatisticsSuccess,
            receiveLifecycleStatisticsFailure,
            this.activeListeners.lifecycle,
            mapServerLifecycleStatisticsResponseToServerLifecycleStatistics
        )
    );

    receiveLifecycleStatisticsFailure$ = createEffect(() => this.handleFailure(
        receiveLifecycleStatisticsFailure,
        "Failed to receive statistics in lifecycle statistics hub"
    ),
        { dispatch: false }
    );

    stopLifecycleStatisticsReceiving$ = createEffect(() => this.handleStopListening(
        environment.lifecycleStatisticsHub,
        stopLifecycleStatisticsReceiving,
        this.activeListeners.lifecycle
    ),
        { dispatch: false }
    );

    //#endregion

    //#region Load Statistics

    startLoadStatisticsReceiving$ = createEffect(() =>
        this.handleStatisticsReceiving<ServerLoadStatisticsResponse>(
            environment.loadStatisticsHub,
            startLoadStatisticsReceiving,
            receiveLoadStatisticsSuccess,
            receiveLoadStatisticsFailure,
            this.activeListeners.load,
            mapServerLoadStatisticsResponseToServerLoadStatistics
        )
    );

    receiveLoadStatisticsFailure$ = this.handleFailure(
        receiveLoadStatisticsFailure,
        "Failed to receive statistics in load statistics hub"
    );

    stopLoadKeyListening$ = this.handleStopListening(
        environment.loadStatisticsHub,
        stopLoadKeyListening,
        this.activeListeners.load
    );

    //#endregion

    //#region Custom Statistics

    startCustomStatisticsReceiving$ = createEffect(() =>
        this.handleStatisticsReceiving<ServerCustomStatisticsResponse>(
            environment.customStatisticsHub,
            startCustomStatisticsReceiving,
            receiveCustomStatisticsSuccess,
            receiveCustomStatisticsFailure,
            this.activeListeners.custom,
            mapServerCustomStatisticsResponseToServerCustomStatistics
        )
    );

    receiveCustomStatisticsFailure$ = this.handleFailure(
        receiveCustomStatisticsFailure,
        "Failed to receive statistics in custom statistics hub"
    );

    stopCustomStatisticsReceiving$ = this.handleStopListening(
        environment.customStatisticsHub,
        stopCustomStatisticsReceiving,
        this.activeListeners.custom
    );

    //#endregion

    //#region Load Amount Statistics

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

    //#region Slot Statistics

    downloadSlotStatistics$ = createEffect(() =>
        this.actions$.pipe(
            ofType(downloadSlotStatistics),
            concatMap((action) =>
                this.apiService.getSlotStatistics(action.key).pipe(
                    map((response) => {
                        this.jsonDownloader.downloadInJson(response, `slot-data-${action.key}`);
                        return of()
                    }
                    ),
                    catchError((error) =>
                        of(getLoadAmountStatisticsInRangeFailure({ error: error.message }))
                    )
                )
            )
        ),
        { dispatch: false }
    );
    downLoadSlotStatisticsFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(downLoadSlotStatisticsFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to download slot statistics : " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    //#endregion
}