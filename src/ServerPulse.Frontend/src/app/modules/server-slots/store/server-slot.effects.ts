import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { catchError, map, mergeMap, of } from "rxjs";
import { createServerSlot, createServerSlotFailure, createServerSlotSuccess, deleteServerSlot, deleteServerSlotFailure, deleteServerSlotSuccess, getServerSlots, getServerSlotsFailure, getServerSlotsSuccess, getServerSlotsWithString, getServerSlotsWithStringFailure, getServerSlotsWithStringSuccess, updateServerSlot, updateServerSlotFailure, updateServerSlotSuccess } from "..";
import { ServerSlotApiService, serverSlotResponseToServerSlot } from "../../shared";

//ServerSlots
@Injectable()
export class ServerSlotEffects {

    constructor(
        private readonly actions$: Actions,
        private readonly apiService: ServerSlotApiService,
    ) { }

    getServerSlots$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getServerSlots),
            mergeMap((action) =>
                this.apiService.getUserServerSlots().pipe(
                    map((response) => {
                        var serverSlots = response.map(x => serverSlotResponseToServerSlot(x));
                        return getServerSlotsSuccess({ serverSlots: serverSlots });
                    }),
                    catchError(error => of(getServerSlotsFailure({ error: error.message })))
                )
            )
        )
    );
    getServerSlotsWithString$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getServerSlotsWithString),
            mergeMap((action) =>
                this.apiService.getUserServerSlotsWithString(action.str).pipe(
                    map((response) => {
                        var serverSlots = response.map(x => serverSlotResponseToServerSlot(x));
                        return getServerSlotsWithStringSuccess({ serverSlots: serverSlots });
                    }),
                    catchError(error => of(getServerSlotsWithStringFailure({ error: error.message })))
                )
            )
        )
    );
    createServerSlot$ = createEffect(() =>
        this.actions$.pipe(
            ofType(createServerSlot),
            mergeMap((action) =>
                this.apiService.createServerSlot(action.createRequest).pipe(
                    map((response) => {
                        var serverSlot = serverSlotResponseToServerSlot(response);
                        return createServerSlotSuccess({ serverSlot: serverSlot });
                    }),
                    catchError(error => of(createServerSlotFailure({ error: error.message })))
                )
            )
        )
    );
    updateServerSlot$ = createEffect(() =>
        this.actions$.pipe(
            ofType(updateServerSlot),
            mergeMap((action) =>
                this.apiService.updateServerSlot(action.updateRequest).pipe(
                    map(() => {
                        return updateServerSlotSuccess({ updateRequest: action.updateRequest });
                    }),
                    catchError(error => of(updateServerSlotFailure({ error: error.message })))
                )
            )
        )
    );
    deleteServerSlot$ = createEffect(() =>
        this.actions$.pipe(
            ofType(deleteServerSlot),
            mergeMap((action) =>
                this.apiService.deleteServerSlot(action.id).pipe(
                    map(() => {
                        return deleteServerSlotSuccess({ id: action.id });
                    }),
                    catchError(error => of(deleteServerSlotFailure({ error: error.message })))
                )
            )
        )
    );
}