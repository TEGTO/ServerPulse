import { CreateServerSlotRequest, ServerSlot, UpdateServerSlotRequest } from "..";
import { createServerSlot, createServerSlotFailure, createServerSlotSuccess, deleteServerSlot, deleteServerSlotFailure, deleteServerSlotSuccess, getServerSlotById, getServerSlotByIdFailure, getServerSlotByIdSuccess, getUserServerSlots, getUserServerSlotsFailure, getUserServerSlotsSuccess, showSlotInfo, showSlotKey, updateServerSlot, updateServerSlotFailure, updateServerSlotSuccess } from "./server-slot.actions";

describe('Server Slot Actions', () => {
    const error = { message: 'An error occurred' };

    describe('Get Server Slot By ID Actions', () => {
        it('should create getServerSlotById action', () => {
            const action = getServerSlotById({ id: 'slot-id' });
            expect(action.type).toBe('[Server Slot] Get Server Slot By Id');
            expect(action.id).toBe('slot-id');
        });

        it('should create getServerSlotByIdSuccess action', () => {
            const serverSlot: ServerSlot = {
                id: 'slot-id',
                userEmail: 'user@example.com',
                name: 'Test Slot',
                slotKey: 'key123',
            };
            const action = getServerSlotByIdSuccess({ serverSlot });
            expect(action.type).toBe('[Server Slot] Get Server Slot By Id Success');
            expect(action.serverSlot).toEqual(serverSlot);
        });

        it('should create getServerSlotByIdFailure action', () => {
            const action = getServerSlotByIdFailure({ error });
            expect(action.type).toBe('[Server Slot] Get Server Slot By Id Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Get User Server Slots Actions', () => {
        it('should create getUserServerSlots action', () => {
            const action = getUserServerSlots({ str: 'search-keyword' });
            expect(action.type).toBe('[Server Slot] Get User Server Slots');
            expect(action.str).toBe('search-keyword');
        });

        it('should create getUserServerSlotsSuccess action', () => {
            const serverSlots: ServerSlot[] = [
                { id: '1', userEmail: 'user1@example.com', name: 'Slot 1', slotKey: 'key1' },
            ];
            const action = getUserServerSlotsSuccess({ serverSlots });
            expect(action.type).toBe('[Server Slot] Get User Server Slots Success');
            expect(action.serverSlots).toEqual(serverSlots);
        });

        it('should create getUserServerSlotsFailure action', () => {
            const action = getUserServerSlotsFailure({ error });
            expect(action.type).toBe('[Server Slot] Get User Server Slots Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Create Server Slot Actions', () => {
        it('should create createServerSlot action', () => {
            const req: CreateServerSlotRequest = { name: 'New Slot' };
            const action = createServerSlot({ req });
            expect(action.type).toBe('[Server Slot] Create A New Server Slot');
            expect(action.req).toEqual(req);
        });

        it('should create createServerSlotSuccess action', () => {
            const serverSlot: ServerSlot = {
                id: '1',
                userEmail: 'user@example.com',
                name: 'New Slot',
                slotKey: 'key1',
            };
            const action = createServerSlotSuccess({ serverSlot });
            expect(action.type).toBe('[Server Slot] Create A New Server Slot Success');
            expect(action.serverSlot).toEqual(serverSlot);
        });

        it('should create createServerSlotFailure action', () => {
            const action = createServerSlotFailure({ error });
            expect(action.type).toBe('[Server Slot] Create A New Server Slot Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Update Server Slot Actions', () => {
        it('should create updateServerSlot action', () => {
            const req: UpdateServerSlotRequest = { id: '1', name: 'Updated Slot' };
            const action = updateServerSlot({ req });
            expect(action.type).toBe('[Server Slot] Update A Server Slot');
            expect(action.req).toEqual(req);
        });

        it('should create updateServerSlotSuccess action', () => {
            const req: UpdateServerSlotRequest = { id: '1', name: 'Updated Slot' };
            const action = updateServerSlotSuccess({ req });
            expect(action.type).toBe('[Server Slot] Update A Server Slot Success');
            expect(action.req).toEqual(req);
        });

        it('should create updateServerSlotFailure action', () => {
            const action = updateServerSlotFailure({ error });
            expect(action.type).toBe('[Server Slot] Update A Server Slot Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Delete Server Slot Actions', () => {
        it('should create deleteServerSlot action', () => {
            const action = deleteServerSlot({ id: 'slot-id' });
            expect(action.type).toBe('[Server Slot] Delete A Server Slot');
            expect(action.id).toBe('slot-id');
        });

        it('should create deleteServerSlotSuccess action', () => {
            const action = deleteServerSlotSuccess({ id: 'slot-id' });
            expect(action.type).toBe('[Server Slot] Delete A Server Slot Success');
            expect(action.id).toBe('slot-id');
        });

        it('should create deleteServerSlotFailure action', () => {
            const action = deleteServerSlotFailure({ error });
            expect(action.type).toBe('[Server Slot] Delete A Server Slot Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Slot Info and Key Actions', () => {
        it('should create showSlotInfo action', () => {
            const action = showSlotInfo({ id: 'slot-id' });
            expect(action.type).toBe('[Server Slot] Show Server Slot Info By Id');
            expect(action.id).toBe('slot-id');
        });

        it('should create showSlotKey action', () => {
            const slot: ServerSlot = { id: 'slot-id', userEmail: 'user@example.com', name: 'Slot', slotKey: 'key123' };
            const action = showSlotKey({ slot });
            expect(action.type).toBe('[Server Slot] Show Server Slot Key');
            expect(action.slot).toEqual(slot);
        });
    });
});
