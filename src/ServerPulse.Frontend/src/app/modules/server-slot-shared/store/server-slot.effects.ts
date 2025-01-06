import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { catchError, EMPTY, map, mergeMap, of, switchMap } from "rxjs";
import { createServerSlot, createServerSlotFailure, createServerSlotSuccess, deleteServerSlot, deleteServerSlotFailure, deleteServerSlotSuccess, getServerSlotById, getServerSlotByIdFailure, getServerSlotByIdSuccess, getUserServerSlots, getUserServerSlotsFailure, getUserServerSlotsSuccess, ServerSlotApiService, ServerSlotDialogManagerService, showSlotInfo, showSlotKey, updateServerSlot, updateServerSlotFailure, updateServerSlotSuccess } from "..";
import { RedirectorService, SnackbarManager } from "../../shared";

@Injectable({
    providedIn: 'root'
})
export class ServerSlotEffects {
    constructor(
        private readonly actions$: Actions,
        private readonly apiService: ServerSlotApiService,
        private readonly snackbarManager: SnackbarManager,
        private readonly redirector: RedirectorService,
        private readonly dialogManager: ServerSlotDialogManagerService
    ) { }

    getServerSlotById$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getServerSlotById),
            mergeMap((action) =>
                this.apiService.getServerSlotById(action.id).pipe(
                    map((response) => {
                        return getServerSlotByIdSuccess({ serverSlot: response });
                    }),
                    catchError(error => of(getServerSlotByIdFailure({ error: error.message })))
                )
            )
        )
    );
    getServerSlotByIdFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getServerSlotByIdFailure),
            switchMap((action) => {
                this.redirector.redirectToHome();
                this.snackbarManager.openErrorSnackbar(["Failed to get server slot by id: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    getUserServerSlots$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getUserServerSlots),
            switchMap((action) =>
                this.apiService.getUserServerSlots(action.str).pipe(
                    map((response) => {
                        return getUserServerSlotsSuccess({ serverSlots: response });
                    }),
                    catchError(error => of(getUserServerSlotsFailure({ error: error.message })))
                )
            )
        )
    );
    getUserServerSlotsFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(getUserServerSlotsFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to get user server slots: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    createServerSlot$ = createEffect(() =>
        this.actions$.pipe(
            ofType(createServerSlot),
            mergeMap((action) =>
                this.apiService.createServerSlot(action.req).pipe(
                    map((response) => {
                        return createServerSlotSuccess({ serverSlot: response });
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
                this.apiService.updateServerSlot(action.req).pipe(
                    map(() => {
                        return updateServerSlotSuccess({ req: action.req });
                    }),
                    catchError(error => of(updateServerSlotFailure({ error: error.message })))
                )
            )
        )
    );
    updateServerSlotFailure$ = createEffect(() =>
        this.actions$.pipe(
            ofType(updateServerSlotFailure),
            switchMap((action) => {
                this.snackbarManager.openErrorSnackbar(["Failed to update a server slot: " + action.error]);
                return of();
            })
        ),
        { dispatch: false }
    );

    deleteServerSlot$ = createEffect(() =>
        this.actions$.pipe(
            ofType(deleteServerSlot),
            mergeMap((action) =>
                this.dialogManager.openDeleteSlotConfirmMenu().afterClosed().pipe(
                    mergeMap((result) => {
                        if (result === true) {
                            return this.apiService.deleteServerSlot(action.id).pipe(
                                map(() => deleteServerSlotSuccess({ id: action.id })),
                                catchError((error) => of(deleteServerSlotFailure({ error: error.message })))
                            );
                        } else {
                            return EMPTY;
                        }
                    })
                )
            )
        )
    );

    showSlotInfo$ = createEffect(() =>
        this.actions$.pipe(
            ofType(showSlotInfo),
            switchMap((action) => {
                this.redirector.redirectTo(`info/${action.id}`);
                return of();
            })
        ),
        { dispatch: false }
    );

    showSlotKey$ = createEffect(() =>
        this.actions$.pipe(
            ofType(showSlotKey),
            switchMap((action) => {
                const key = action.slot.slotKey;
                this.snackbarManager.openInfoCopySnackbar(`🔑: ${key}`, key, 10);
                return of();
            })
        ),
        { dispatch: false }
    );
}