import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { catchError, map, of, switchMap, tap } from "rxjs";
import { RealTimeStatisticsCollector, subscribeToLoadStatistics, subscribeToLoadStatisticsFailure, subscribeToLoadStatisticsSuccess, subscribeToSlotStatistics, subscribeToSlotStatisticsFailure, subscribeToSlotStatisticsSuccess } from "../..";
import { environment } from "../../../../../environment/environment";
import { StatisticsApiService } from "../../../shared";

@Injectable()
export class ServerSlotStatisticsEffects {
    constructor(
        private readonly actions$: Actions,
        private readonly apiService: StatisticsApiService,
        private readonly statisticsCollector: RealTimeStatisticsCollector,
    ) { }

    //Statistics
    subscribeToSlotStatistics$ = createEffect(() =>
        this.actions$.pipe(
            ofType(subscribeToSlotStatistics),
            switchMap((action) =>
                this.statisticsCollector.startConnection(environment.statisticsHub).pipe(
                    tap(() => {
                        this.statisticsCollector.startListen(environment.statisticsHub, action.slotKey);
                    }),
                    switchMap(() =>
                        this.statisticsCollector.receiveStatistics(environment.statisticsHub).pipe(
                            map((receiveStatistics) =>
                                subscribeToSlotStatisticsSuccess({
                                    lastStatistics: receiveStatistics
                                })
                            )
                        )
                    ),
                    catchError((error) => of(subscribeToSlotStatisticsFailure({ error })))
                )
            )
        )
    );

    //Load Statistics
    subscribeToLoadStatistics$ = createEffect(() =>
        this.actions$.pipe(
            ofType(subscribeToLoadStatistics),
            switchMap((action) =>
                this.statisticsCollector.startConnection(environment.loadStatisticsHub).pipe(
                    tap(() => {
                        this.statisticsCollector.startListen(environment.loadStatisticsHub, action.slotKey);
                    }),
                    switchMap(() =>
                        this.statisticsCollector.receiveStatistics(environment.loadStatisticsHub).pipe(
                            map((receiveLoadStatistics) =>
                                subscribeToLoadStatisticsSuccess({
                                    lastLoadStatistics: receiveLoadStatistics
                                })
                            )
                        )
                    ),
                    catchError((error) => of(subscribeToLoadStatisticsFailure({ error })))
                )
            )
        )
    );
}