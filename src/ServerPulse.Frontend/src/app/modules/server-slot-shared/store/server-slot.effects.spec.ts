/* eslint-disable @typescript-eslint/no-explicit-any */
import { fakeAsync, TestBed, tick } from "@angular/core/testing";
import { provideMockActions } from "@ngrx/effects/testing";
import { Observable, of, throwError } from "rxjs";
import { CreateServerSlotRequest, ServerSlot, ServerSlotApiService, ServerSlotDialogManagerService, UpdateServerSlotRequest } from "..";
import { RedirectorService, SnackbarManager } from "../../shared";
import { createServerSlot, createServerSlotFailure, createServerSlotSuccess, deleteServerSlot, deleteServerSlotSuccess, getServerSlotById, getServerSlotByIdFailure, getServerSlotByIdSuccess, getUserServerSlots, getUserServerSlotsFailure, getUserServerSlotsSuccess, showSlotInfo, showSlotKey, updateServerSlot, updateServerSlotFailure, updateServerSlotSuccess } from "./server-slot.actions";
import { ServerSlotEffects } from "./server-slot.effects";

describe('ServerSlotEffects', () => {
    let actions$: Observable<any>;
    let effects: ServerSlotEffects;

    let apiServiceSpy: jasmine.SpyObj<ServerSlotApiService>;
    let snackbarManagerSpy: jasmine.SpyObj<SnackbarManager>;
    let redirectorSpy: jasmine.SpyObj<RedirectorService>;
    let dialogManagerSpy: jasmine.SpyObj<ServerSlotDialogManagerService>;

    beforeEach(() => {
        apiServiceSpy = jasmine.createSpyObj<ServerSlotApiService>(
            ['getServerSlotById', 'getUserServerSlots', 'createServerSlot', 'updateServerSlot', 'deleteServerSlot']
        );
        snackbarManagerSpy = jasmine.createSpyObj<SnackbarManager>(['openErrorSnackbar', 'openInfoSnackbar', 'openInfoCopySnackbar']);
        redirectorSpy = jasmine.createSpyObj<RedirectorService>(['redirectTo']);
        dialogManagerSpy = jasmine.createSpyObj<ServerSlotDialogManagerService>(['openDeleteSlotConfirmMenu']);

        TestBed.configureTestingModule({
            providers: [
                ServerSlotEffects,
                provideMockActions(() => actions$),
                { provide: ServerSlotApiService, useValue: apiServiceSpy },
                { provide: SnackbarManager, useValue: snackbarManagerSpy },
                { provide: RedirectorService, useValue: redirectorSpy },
                { provide: ServerSlotDialogManagerService, useValue: dialogManagerSpy },
            ],
        });

        effects = TestBed.inject(ServerSlotEffects);
    });

    it('should be created', () => {
        expect(effects).toBeTruthy();
    });

    describe('getServerSlotById$', () => {
        it('should dispatch getServerSlotByIdSuccess on success', () => {
            const serverSlot: ServerSlot = { id: '1', userEmail: 'test@example.com', name: 'Test Slot', slotKey: 'key' };
            const action = getServerSlotById({ id: '1' });
            const outcome = getServerSlotByIdSuccess({ serverSlot });

            apiServiceSpy.getServerSlotById.and.returnValue(of(serverSlot));

            actions$ = of(action);

            effects.getServerSlotById$.subscribe((result) => {
                expect(result).toEqual(outcome);
                expect(apiServiceSpy.getServerSlotById).toHaveBeenCalledWith('1');
            });
        });

        it('should dispatch getServerSlotByIdFailure on failure', () => {
            const error = { message: 'Failed to fetch' };
            const action = getServerSlotById({ id: '1' });
            const outcome = getServerSlotByIdFailure({ error: error.message });

            apiServiceSpy.getServerSlotById.and.returnValue(throwError(() => error));

            actions$ = of(action);

            effects.getServerSlotById$.subscribe((result) => {
                expect(result).toEqual(outcome);
                expect(apiServiceSpy.getServerSlotById).toHaveBeenCalledWith('1');
            });
        });
    });

    describe('getUserServerSlots$', () => {
        it('should dispatch getUserServerSlotsSuccess on success', () => {
            const serverSlots: ServerSlot[] = [{ id: '1', userEmail: 'test@example.com', name: 'Test Slot', slotKey: 'key' }];
            const action = getUserServerSlots({ str: 'keyword' });
            const outcome = getUserServerSlotsSuccess({ serverSlots });

            apiServiceSpy.getUserServerSlots.and.returnValue(of(serverSlots));

            actions$ = of(action);

            effects.getUserServerSlots$.subscribe((result) => {
                expect(result).toEqual(outcome);
                expect(apiServiceSpy.getUserServerSlots).toHaveBeenCalledWith('keyword');
            });
        });

        it('should dispatch getUserServerSlotsFailure on failure', () => {
            const error = { message: 'Failed to fetch' };
            const action = getUserServerSlots({ str: 'keyword' });
            const outcome = getUserServerSlotsFailure({ error: error.message });

            apiServiceSpy.getUserServerSlots.and.returnValue(throwError(() => error));

            actions$ = of(action);

            effects.getUserServerSlots$.subscribe((result) => {
                expect(result).toEqual(outcome);
                expect(apiServiceSpy.getUserServerSlots).toHaveBeenCalledWith('keyword');
            });
        });
    });

    describe('createServerSlot$', () => {
        it('should dispatch createServerSlotSuccess on success', () => {
            const request: CreateServerSlotRequest = { name: 'New Slot' };
            const serverSlot: ServerSlot = { id: '1', userEmail: 'test@example.com', name: 'New Slot', slotKey: 'key' };
            const action = createServerSlot({ req: request });
            const outcome = createServerSlotSuccess({ serverSlot });

            apiServiceSpy.createServerSlot.and.returnValue(of(serverSlot));

            actions$ = of(action);

            effects.createServerSlot$.subscribe((result) => {
                expect(result).toEqual(outcome);
                expect(apiServiceSpy.createServerSlot).toHaveBeenCalledWith(request);
            });
        });

        it('should dispatch createServerSlotFailure on failure', () => {
            const request: CreateServerSlotRequest = { name: 'New Slot' };
            const error = { message: 'Failed to create' };
            const action = createServerSlot({ req: request });
            const outcome = createServerSlotFailure({ error: error.message });

            apiServiceSpy.createServerSlot.and.returnValue(throwError(() => error));

            actions$ = of(action);

            effects.createServerSlot$.subscribe((result) => {
                expect(result).toEqual(outcome);
                expect(apiServiceSpy.createServerSlot).toHaveBeenCalledWith(request);
            });
        });
    });

    describe('updateServerSlot$', () => {
        it('should dispatch updateServerSlotSuccess on success', fakeAsync(() => {
            const request: UpdateServerSlotRequest = { id: '1', name: 'Updated Slot' };
            const action = updateServerSlot({ req: request });
            const outcome = updateServerSlotSuccess({ req: request });

            apiServiceSpy.updateServerSlot.and.returnValue(of());

            actions$ = of(action);

            effects.updateServerSlot$.subscribe((result) => {
                expect(result).toEqual(outcome);
            });

            tick();

            expect(apiServiceSpy.updateServerSlot).toHaveBeenCalledWith(request);
        }));

        it('should dispatch updateServerSlotFailure on failure', () => {
            const request: UpdateServerSlotRequest = { id: '1', name: 'Updated Slot' };
            const error = { message: 'Failed to update' };
            const action = updateServerSlot({ req: request });
            const outcome = updateServerSlotFailure({ error: error.message });

            apiServiceSpy.updateServerSlot.and.returnValue(throwError(() => error));

            actions$ = of(action);

            effects.updateServerSlot$.subscribe((result) => {
                expect(result).toEqual(outcome);
                expect(apiServiceSpy.updateServerSlot).toHaveBeenCalledWith(request);
            });
        });
    });

    describe('deleteServerSlot$', () => {
        it('should dispatch deleteServerSlotSuccess on confirmation', fakeAsync(() => {
            const action = deleteServerSlot({ id: '1' });
            const outcome = deleteServerSlotSuccess({ id: '1' });

            dialogManagerSpy.openDeleteSlotConfirmMenu.and.returnValue({
                afterClosed: () => of(true),
            } as any);
            apiServiceSpy.deleteServerSlot.and.returnValue(of());

            actions$ = of(action);

            effects.deleteServerSlot$.subscribe((result) => {
                expect(result).toEqual(outcome);
            });

            tick();

            expect(apiServiceSpy.deleteServerSlot).toHaveBeenCalledWith('1');
        }));

        it('should not proceed when confirmation is cancelled', fakeAsync(() => {
            const action = deleteServerSlot({ id: '1' });

            dialogManagerSpy.openDeleteSlotConfirmMenu.and.returnValue({
                afterClosed: () => of(false),
            } as any);

            actions$ = of(action);

            effects.deleteServerSlot$.subscribe({
                next: () => fail('Expected no action to be dispatched'),
                error: () => fail('Expected no error'),
            });

            tick();

            expect(apiServiceSpy.deleteServerSlot).not.toHaveBeenCalled();
        }));
    });

    describe('showSlotInfo$', () => {
        it('should redirect to the correct URL', fakeAsync(() => {
            const action = showSlotInfo({ id: '1' });

            actions$ = of(action);

            effects.showSlotInfo$.subscribe();

            tick();

            expect(redirectorSpy.redirectTo).toHaveBeenCalledWith('info/1');
        }));
    });

    describe('showSlotKey$', () => {
        it('should display the slot key in a snackbar', fakeAsync(() => {
            const slot: ServerSlot = { id: '1', userEmail: 'test@example.com', name: 'Slot', slotKey: 'key' };
            const action = showSlotKey({ slot });

            actions$ = of(action);

            effects.showSlotKey$.subscribe();

            tick();

            expect(snackbarManagerSpy.openInfoCopySnackbar).toHaveBeenCalledWith('🔑: key', 'key', 10);
        }));
    });
});
