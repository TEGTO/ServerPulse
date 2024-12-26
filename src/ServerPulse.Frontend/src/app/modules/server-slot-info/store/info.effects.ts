import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { catchError, map, mergeMap, of, switchMap } from "rxjs";
import { getDailyLoadAmountStatistics, getDailyLoadAmountStatisticsFailure, getDailyLoadAmountStatisticsSuccess, getLoadAmountStatisticsInRange, getLoadAmountStatisticsInRangeFailure, getLoadAmountStatisticsInRangeSuccess, getSomeCustomEvents, getSomeCustomEventsFailure, getSomeCustomEventsSuccess, getSomeLoadEvents, getSomeLoadEventsFailure, getSomeLoadEventsSuccess, ServerSlotInfoDialogManagerService, showCustomDetailsEvent } from "..";
import { AnalyzerApiService, CustomEvent, LoadEvent } from "../../analyzer";
import { SnackbarManager } from "../../shared";

@Injectable({
    providedIn: 'root'
})
export class ServerSlotInfoEffects {
    private readonly getSomeLoadEventsCache = new Map<number, LoadEvent[]>();
    private readonly getSomeCustomEventsCache = new Map<number, CustomEvent[]>();

    constructor(
        private readonly actions$: Actions,
        private readonly analyzerApiService: AnalyzerApiService,
        private readonly snackbarManager: SnackbarManager,
        private readonly dialogManger: ServerSlotInfoDialogManagerService
    ) { }

    getSomeLoadEvents$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getSomeLoadEvents),
            mergeMap((action) => {
                const cacheKey = action.req.startDate.getTime();

                if (!this.getSomeLoadEventsCache.has(cacheKey)) {
                    return this.analyzerApiService.getSomeLoadEvents(action.req).pipe(
                        map((response) => {
                            this.getSomeLoadEventsCache.set(cacheKey, response);
                            return getSomeLoadEventsSuccess({ events: response });
                        }),
                        catchError(error => of(getSomeLoadEventsFailure({ error: error.message })))
                    );
                } else {
                    return of(getSomeLoadEventsSuccess({ events: this.getSomeLoadEventsCache.get(cacheKey)! }));
                }
            })
        )
    );
    getSomeLoadEventsFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getSomeLoadEventsFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to get load events: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    getSomeCustomEvents$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getSomeCustomEvents),
            mergeMap((action) => {
                const cacheKey = action.req.startDate.getTime();

                if (!this.getSomeCustomEventsCache.has(cacheKey)) {
                    return this.analyzerApiService.getSomeCustomEvents(action.req).pipe(
                        map((response) => {
                            this.getSomeCustomEventsCache.set(cacheKey, response);
                            return getSomeCustomEventsSuccess({ events: response });
                        }),
                        catchError(error => of(getSomeCustomEventsFailure({ error: error.message })))
                    );
                } else {
                    return of(getSomeCustomEventsSuccess({ events: this.getSomeCustomEventsCache.get(cacheKey)! }));
                }
            })
        )
    );
    getSomeCustomEventsFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getSomeCustomEventsFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to get custom events: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    getDailyLoadAmountStatistics$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getDailyLoadAmountStatistics),
            mergeMap((action) =>
                this.analyzerApiService.getDailyLoadStatistics(action.key).pipe(
                    map((response) => {
                        return getDailyLoadAmountStatisticsSuccess({ statistics: response });
                    }),
                    catchError(error => of(getDailyLoadAmountStatisticsFailure({ error: error.message })))
                )
            )
        )
    );
    getDailyLoadAmountStatisticsFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getDailyLoadAmountStatisticsFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to get daily load amount statistics: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    getLoadAmountStatisticsInRange$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getLoadAmountStatisticsInRange),
            mergeMap((action) =>
                this.analyzerApiService.getLoadAmountStatisticsInRange(action.req).pipe(
                    map((response) => {
                        return getLoadAmountStatisticsInRangeSuccess({ statistics: response });
                    }),
                    catchError(error => of(getLoadAmountStatisticsInRangeFailure({ error: error.message })))
                )
            )
        )
    );
    getLoadAmountStatisticsInRangeFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getLoadAmountStatisticsInRangeFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to get load amount statistics in range: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    showCustomDetailsEvent$ = createEffect(() =>
        this.actions$.pipe(
            ofType(showCustomDetailsEvent),
            switchMap((action) => {
                this.dialogManger.openCustomEventDetails(action.event.serializedMessage);
                return of();
            })
        ),
        { dispatch: false }
    );
}