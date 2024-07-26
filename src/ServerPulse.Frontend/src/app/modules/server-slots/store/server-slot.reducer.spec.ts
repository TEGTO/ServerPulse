import { ServerSlot, UpdateServerSlotRequest, applyUpdateRequestOnServerSlot } from "../../shared";
import { createServerSlotFailure, createServerSlotSuccess, deleteServerSlotFailure, deleteServerSlotSuccess, getServerSlotsFailure, getServerSlotsSuccess, getServerSlotsWithStringFailure, getServerSlotsWithStringSuccess, updateServerSlotFailure, updateServerSlotSuccess } from "./server-slot.actions";
import { ServerSlotState, serverSlotReducer } from "./server-slot.reducer";

describe('ServerSlot Reducer', () => {
    const initialState: ServerSlotState = {
        serverSlots: [],
        error: null
    };

    const mockServerSlots: ServerSlot[] = [
        { id: '1', userEmail: 'user1@example.com', name: 'Slot 1', slotKey: 'key1' },
        { id: '2', userEmail: 'user2@example.com', name: 'Slot 2', slotKey: 'key2' }
    ];

    it('should return the initial state', () => {
        const action = { type: 'Unknown' } as any;
        const state = serverSlotReducer(initialState, action);
        expect(state).toBe(initialState);
    });

    it('should handle getServerSlotsSuccess', () => {
        const action = getServerSlotsSuccess({ serverSlots: mockServerSlots });
        const state = serverSlotReducer(initialState, action);
        expect(state.serverSlots).toEqual(mockServerSlots);
        expect(state.error).toBeNull();
    });

    it('should handle getServerSlotsFailure', () => {
        const error = 'Error';
        const action = getServerSlotsFailure({ error });
        const state = serverSlotReducer(initialState, action);
        expect(state.serverSlots).toEqual([]);
        expect(state.error).toEqual(error);
    });

    it('should handle getServerSlotsWithStringSuccess', () => {
        const action = getServerSlotsWithStringSuccess({ serverSlots: mockServerSlots });
        const state = serverSlotReducer(initialState, action);
        expect(state.serverSlots).toEqual(mockServerSlots);
        expect(state.error).toBeNull();
    });

    it('should handle getServerSlotsWithStringFailure', () => {
        const error = 'Error';
        const action = getServerSlotsWithStringFailure({ error });
        const state = serverSlotReducer(initialState, action);
        expect(state.serverSlots).toEqual([]);
        expect(state.error).toEqual(error);
    });

    it('should handle createServerSlotSuccess', () => {
        const newServerSlot: ServerSlot = { id: '3', userEmail: 'user3@example.com', name: 'Slot 3', slotKey: 'key3' };
        const action = createServerSlotSuccess({ serverSlot: newServerSlot });
        const state = serverSlotReducer(initialState, action);
        expect(state.serverSlots).toEqual([newServerSlot]);
        expect(state.error).toBeNull();
    });

    it('should handle createServerSlotFailure', () => {
        const error = 'Error';
        const action = createServerSlotFailure({ error });
        const state = serverSlotReducer(initialState, action);
        expect(state.serverSlots).toEqual([]);
        expect(state.error).toEqual(error);
    });

    it('should handle updateServerSlotSuccess', () => {
        const updateRequest: UpdateServerSlotRequest = { id: '1', name: 'Updated Slot 1' };
        const updatedServerSlot = applyUpdateRequestOnServerSlot(mockServerSlots[0], updateRequest);
        const action = updateServerSlotSuccess({ updateRequest });
        const state = serverSlotReducer({ ...initialState, serverSlots: mockServerSlots }, action);
        expect(state.serverSlots).toEqual([updatedServerSlot, mockServerSlots[1]]);
        expect(state.error).toBeNull();
    });

    it('should handle updateServerSlotFailure', () => {
        const error = 'Error';
        const action = updateServerSlotFailure({ error });
        const state = serverSlotReducer(initialState, action);
        expect(state.error).toEqual(error);
    });

    it('should handle deleteServerSlotSuccess', () => {
        const id = '1';
        const action = deleteServerSlotSuccess({ id });
        const state = serverSlotReducer({ ...initialState, serverSlots: mockServerSlots }, action);
        expect(state.serverSlots).toEqual([mockServerSlots[1]]);
        expect(state.error).toBeNull();
    });

    it('should handle deleteServerSlotFailure', () => {
        const error = 'Error';
        const action = deleteServerSlotFailure({ error });
        const state = serverSlotReducer(initialState, action);
        expect(state.error).toEqual(error);
    });
});