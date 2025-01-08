/* eslint-disable @typescript-eslint/no-explicit-any */
import { createServerSlotSuccess, deleteServerSlotSuccess, getServerSlotByIdSuccess, getUserServerSlotsSuccess, ServerSlot, serverSlotReducer, ServerSlotState, updateServerSlotSuccess, UpdateSlotRequest } from "..";

describe('ServerSlot Reducer', () => {
    const initialState: ServerSlotState = {
        serverSlots: [],
    };

    it('should return the initial state by default', () => {
        const action = { type: 'UNKNOWN' } as any;
        const state = serverSlotReducer(initialState, action);

        expect(state).toEqual(initialState);
    });

    describe('getServerSlotByIdSuccess', () => {
        it('should add or update a server slot in the state', () => {
            const serverSlot: ServerSlot = { id: '1', userEmail: 'test@example.com', name: 'Test Slot', slotKey: 'key' };
            const action = getServerSlotByIdSuccess({ serverSlot });

            const state = serverSlotReducer(initialState, action);

            expect(state.serverSlots).toContain(serverSlot);
            expect(state.serverSlots.length).toBe(1);
        });

        it('should replace an existing server slot with the same ID', () => {
            const existingSlot: ServerSlot = { id: '1', userEmail: 'old@example.com', name: 'Old Slot', slotKey: 'oldKey' };
            const newSlot: ServerSlot = { id: '1', userEmail: 'test@example.com', name: 'Updated Slot', slotKey: 'key' };
            const initialStateWithSlot: ServerSlotState = {
                serverSlots: [existingSlot],
            };

            const action = getServerSlotByIdSuccess({ serverSlot: newSlot });
            const state = serverSlotReducer(initialStateWithSlot, action);

            expect(state.serverSlots).toContain(newSlot);
            expect(state.serverSlots).not.toContain(existingSlot);
            expect(state.serverSlots.length).toBe(1);
        });
    });

    describe('getUserServerSlotsSuccess', () => {
        it('should replace the server slots in the state', () => {
            const serverSlots: ServerSlot[] = [
                { id: '1', userEmail: 'test1@example.com', name: 'Slot 1', slotKey: 'key1' },
                { id: '2', userEmail: 'test2@example.com', name: 'Slot 2', slotKey: 'key2' },
            ];
            const action = getUserServerSlotsSuccess({ serverSlots });

            const state = serverSlotReducer(initialState, action);

            expect(state.serverSlots).toEqual(serverSlots);
        });
    });

    describe('createServerSlotSuccess', () => {
        it('should add a new server slot to the state', () => {
            const serverSlot: ServerSlot = { id: '1', userEmail: 'test@example.com', name: 'New Slot', slotKey: 'key' };
            const action = createServerSlotSuccess({ serverSlot });

            const state = serverSlotReducer(initialState, action);

            expect(state.serverSlots).toContain(serverSlot);
            expect(state.serverSlots.length).toBe(1);
        });

        it('should add the new server slot to the beginning of the list', () => {
            const existingSlot: ServerSlot = { id: '2', userEmail: 'existing@example.com', name: 'Existing Slot', slotKey: 'existingKey' };
            const newSlot: ServerSlot = { id: '1', userEmail: 'test@example.com', name: 'New Slot', slotKey: 'key' };
            const initialStateWithSlot: ServerSlotState = {
                serverSlots: [existingSlot],
            };

            const action = createServerSlotSuccess({ serverSlot: newSlot });
            const state = serverSlotReducer(initialStateWithSlot, action);

            expect(state.serverSlots[0]).toBe(newSlot);
            expect(state.serverSlots[1]).toBe(existingSlot);
        });
    });

    describe('updateServerSlotSuccess', () => {
        it('should update the server slot in the state', () => {
            const existingSlot: ServerSlot = { id: '1', userEmail: 'test@example.com', name: 'Old Slot', slotKey: 'key' };
            const updateRequest: UpdateSlotRequest = { id: '1', name: 'Updated Slot' };
            const initialStateWithSlot: ServerSlotState = {
                serverSlots: [existingSlot],
            };

            const action = updateServerSlotSuccess({ req: updateRequest });
            const state = serverSlotReducer(initialStateWithSlot, action);

            expect(state.serverSlots[0].name).toBe('Updated Slot');
        });

        it('should leave other server slots unchanged', () => {
            const existingSlot: ServerSlot = { id: '1', userEmail: 'test@example.com', name: 'Old Slot', slotKey: 'key' };
            const otherSlot: ServerSlot = { id: '2', userEmail: 'other@example.com', name: 'Other Slot', slotKey: 'otherKey' };
            const updateRequest: UpdateSlotRequest = { id: '1', name: 'Updated Slot' };
            const initialStateWithSlots: ServerSlotState = {
                serverSlots: [existingSlot, otherSlot],
            };

            const action = updateServerSlotSuccess({ req: updateRequest });
            const state = serverSlotReducer(initialStateWithSlots, action);

            expect(state.serverSlots[0].name).toBe('Updated Slot');
            expect(state.serverSlots[1]).toBe(otherSlot);
        });
    });

    describe('deleteServerSlotSuccess', () => {
        it('should remove the server slot with the specified ID from the state', () => {
            const slotToDelete: ServerSlot = { id: '1', userEmail: 'test@example.com', name: 'Slot to Delete', slotKey: 'key' };
            const otherSlot: ServerSlot = { id: '2', userEmail: 'other@example.com', name: 'Other Slot', slotKey: 'otherKey' };
            const initialStateWithSlots: ServerSlotState = {
                serverSlots: [slotToDelete, otherSlot],
            };

            const action = deleteServerSlotSuccess({ id: '1' });
            const state = serverSlotReducer(initialStateWithSlots, action);

            expect(state.serverSlots).not.toContain(slotToDelete);
            expect(state.serverSlots).toContain(otherSlot);
        });

        it('should not affect the state if the ID does not exist', () => {
            const existingSlot: ServerSlot = { id: '1', userEmail: 'test@example.com', name: 'Existing Slot', slotKey: 'key' };
            const initialStateWithSlot: ServerSlotState = {
                serverSlots: [existingSlot],
            };

            const action = deleteServerSlotSuccess({ id: '2' });
            const state = serverSlotReducer(initialStateWithSlot, action);

            expect(state.serverSlots).toEqual([existingSlot]);
        });
    });
});
