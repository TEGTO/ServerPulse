import { HttpClientTestingModule } from "@angular/common/http/testing";
import { TestBed } from "@angular/core/testing";
import { provideMockActions } from "@ngrx/effects/testing";
import { Observable, of, throwError } from "rxjs";
import { CreateServerSlotRequest, ServerSlotApiService, ServerSlotResponse, serverSlotResponseToServerSlot, UpdateServerSlotRequest } from "../../../shared";
import { createServerSlot, createServerSlotFailure, createServerSlotSuccess, deleteServerSlot, deleteServerSlotFailure, deleteServerSlotSuccess, getServerSlots, getServerSlotsFailure, getServerSlotsSuccess, getServerSlotsWithString, getServerSlotsWithStringFailure, getServerSlotsWithStringSuccess, updateServerSlot, updateServerSlotFailure, updateServerSlotSuccess } from "../../index";
import { ServerSlotEffects } from "./server-slot.effects";

describe('ServerSlotEffects', () => {
    let actions$: Observable<any>;
    let effects: ServerSlotEffects;
    let mockApiService: jasmine.SpyObj<ServerSlotApiService>;

    beforeEach(() => {
        mockApiService = jasmine.createSpyObj('ServerSlotApiService', ['getUserServerSlots', 'getUserServerSlotsWithString', 'createServerSlot', 'updateServerSlot', 'deleteServerSlot']);

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                ServerSlotEffects,
                provideMockActions(() => actions$),
                { provide: ServerSlotApiService, useValue: mockApiService }
            ]
        });

        effects = TestBed.inject(ServerSlotEffects);
    });

    describe('getServerSlots$', () => {
        it('should dispatch getServerSlotsSuccess on successful getServerSlots', (done) => {
            const serverSlotsResponse: ServerSlotResponse[] = [{ id: '1', userEmail: 'user@example.com', name: 'Slot 1', slotKey: 'key1' }];
            const serverSlots = serverSlotsResponse.map(serverSlotResponseToServerSlot);
            const action = getServerSlots();
            const outcome = getServerSlotsSuccess({ serverSlots });

            actions$ = of(action);
            mockApiService.getUserServerSlots.and.returnValue(of(serverSlotsResponse));

            effects.getServerSlots$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.getUserServerSlots).toHaveBeenCalled();
                done();
            });
        });
        it('should dispatch getServerSlotsFailure on failed getServerSlots', (done) => {
            const action = getServerSlots();
            const error = new Error('Error!');
            const outcome = getServerSlotsFailure({ error: error.message });

            actions$ = of(action);
            mockApiService.getUserServerSlots.and.returnValue(throwError(error));

            effects.getServerSlots$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.getUserServerSlots).toHaveBeenCalled();
                done();
            });
        });
    });

    describe('getServerSlotsWithString$', () => {
        it('should dispatch getServerSlotsWithStringSuccess on successful getServerSlotsWithString', (done) => {
            const serverSlotsResponse: ServerSlotResponse[] = [{ id: '1', userEmail: 'user@example.com', name: 'Slot 1', slotKey: 'key1' }];
            const serverSlots = serverSlotsResponse.map(serverSlotResponseToServerSlot);
            const action = getServerSlotsWithString({ str: 'test' });
            const outcome = getServerSlotsWithStringSuccess({ serverSlots });

            actions$ = of(action);
            mockApiService.getUserServerSlotsWithString.and.returnValue(of(serverSlotsResponse));

            effects.getServerSlotsWithString$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.getUserServerSlotsWithString).toHaveBeenCalled();
                done();
            });
        });
        it('should dispatch getServerSlotsWithStringFailure on failed getServerSlotsWithString', (done) => {
            const action = getServerSlotsWithString({ str: 'test' });
            const error = new Error('Error!');
            const outcome = getServerSlotsWithStringFailure({ error: error.message });

            actions$ = of(action);
            mockApiService.getUserServerSlotsWithString.and.returnValue(throwError(error));

            effects.getServerSlotsWithString$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.getUserServerSlotsWithString).toHaveBeenCalled();
                done();
            });
        });
    });

    describe('createServerSlot$', () => {
        it('should dispatch createServerSlotSuccess on successful createServerSlot', (done) => {
            const createRequest: CreateServerSlotRequest = { name: 'Slot 1' };
            const serverSlotResponse: ServerSlotResponse = { id: '1', userEmail: 'user@example.com', name: 'Slot 1', slotKey: 'key1' };
            const serverSlot = serverSlotResponseToServerSlot(serverSlotResponse);
            const action = createServerSlot({ createRequest });
            const outcome = createServerSlotSuccess({ serverSlot });

            actions$ = of(action);
            mockApiService.createServerSlot.and.returnValue(of(serverSlotResponse));

            effects.createServerSlot$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.createServerSlot).toHaveBeenCalled();
                done();
            });
        });
        it('should dispatch createServerSlotFailure on failed createServerSlot', (done) => {
            const createRequest: CreateServerSlotRequest = { name: 'Slot 1' };
            const action = createServerSlot({ createRequest });
            const error = new Error('Error!');
            const outcome = createServerSlotFailure({ error: error.message });

            actions$ = of(action);
            mockApiService.createServerSlot.and.returnValue(throwError(error));

            effects.createServerSlot$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.createServerSlot).toHaveBeenCalled();
                done();
            });
        });
    });

    describe('updateServerSlot$', () => {
        it('should dispatch updateServerSlotSuccess on successful updateServerSlot', (done) => {
            const updateRequest: UpdateServerSlotRequest = { id: '1', name: 'Updated Slot' };
            const action = updateServerSlot({ updateRequest });
            const outcome = updateServerSlotSuccess({ updateRequest });

            actions$ = of(action);
            mockApiService.updateServerSlot.and.returnValue(of({}));

            effects.updateServerSlot$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.updateServerSlot).toHaveBeenCalled();
                done();
            });
        });
        it('should dispatch updateServerSlotFailure on failed updateServerSlot', (done) => {
            const updateRequest: UpdateServerSlotRequest = { id: '1', name: 'Updated Slot' };
            const action = updateServerSlot({ updateRequest });
            const error = new Error('Error!');
            const outcome = updateServerSlotFailure({ error: error.message });

            actions$ = of(action);
            mockApiService.updateServerSlot.and.returnValue(throwError(error));

            effects.updateServerSlot$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.updateServerSlot).toHaveBeenCalled();
                done();
            });
        });
    });

    describe('deleteServerSlot$', () => {
        it('should dispatch deleteServerSlotSuccess on successful deleteServerSlot', (done) => {
            const id = '1';
            const action = deleteServerSlot({ id });
            const outcome = deleteServerSlotSuccess({ id });

            actions$ = of(action);
            mockApiService.deleteServerSlot.and.returnValue(of({}));

            effects.deleteServerSlot$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.deleteServerSlot).toHaveBeenCalled();
                done();
            });
        });
        it('should dispatch deleteServerSlotFailure on failed deleteServerSlot', (done) => {
            const id = '1';
            const action = deleteServerSlot({ id });
            const error = new Error('Error!');
            const outcome = deleteServerSlotFailure({ error: error.message });

            actions$ = of(action);
            mockApiService.deleteServerSlot.and.returnValue(throwError(error));

            effects.deleteServerSlot$.subscribe(result => {
                expect(result).toEqual(outcome);
                expect(mockApiService.deleteServerSlot).toHaveBeenCalled();
                done();
            });
        });
    });
});