import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { catchError, map, mergeMap, of, switchMap } from "rxjs";
import { getDailyLoadAmountStatistics, getDailyLoadAmountStatisticsFailure, getDailyLoadAmountStatisticsSuccess, getLoadAmountStatisticsInRange, getLoadAmountStatisticsInRangeFailure, getLoadAmountStatisticsInRangeSuccess, getSomeLoadEvents, getSomeLoadEventsFailure, getSomeLoadEventsSuccess } from "..";
import { AnalyzerApiService } from "../../analyzer";
import { SnackbarManager } from "../../shared";

@Injectable({
    providedIn: 'root'
})
export class ServerSlotInfoEffects {
    constructor(
        private readonly actions$: Actions,
        private readonly analyzerApiService: AnalyzerApiService,
        private readonly snackbarManager: SnackbarManager
    ) { }

    getSomeLoadEvents$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getSomeLoadEvents),
            mergeMap((action) =>
                this.analyzerApiService.getSomeLoadEvents(action.req).pipe(
                    map((response) => {
                        return getSomeLoadEventsSuccess({ events: response });
                    }),
                    catchError(error => of(getSomeLoadEventsFailure({ error: error.message })))
                )
            )
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
}