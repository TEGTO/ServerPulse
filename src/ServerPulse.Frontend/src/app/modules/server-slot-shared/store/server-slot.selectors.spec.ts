import { selectServerSlotById, selectServerSlots, selectServerSlotState, ServerSlotState } from "..";

describe('ServerSlot Selectors', () => {
    const initialState: ServerSlotState = {
        serverSlots: [],
    };

    const populatedState: ServerSlotState = {
        serverSlots: [
            { id: '1', userEmail: 'user1@example.com', name: 'Slot 1', slotKey: 'key1' },
            { id: '2', userEmail: 'user2@example.com', name: 'Slot 2', slotKey: 'key2' },
        ],
    };

    it('should select the server slot state', () => {
        const result = selectServerSlotState.projector(initialState);
        expect(result).toEqual(initialState);
    });

    describe('selectServerSlots', () => {
        it('should select all server slots from the state', () => {
            const result = selectServerSlots.projector(populatedState);
            expect(result).toEqual(populatedState.serverSlots);
        });

        it('should return an empty array if there are no server slots', () => {
            const result = selectServerSlots.projector(initialState);
            expect(result).toEqual([]);
        });
    });

    describe('selectServerSlotById', () => {
        it('should select the server slot by ID when it exists', () => {
            const result = selectServerSlotById('1').projector(populatedState);
            expect(result).toEqual(populatedState.serverSlots[0]);
        });

        it('should return undefined if the server slot ID does not exist', () => {
            const result = selectServerSlotById('3').projector(populatedState);
            expect(result).toBeUndefined();
        });

        it('should return undefined if the state has no server slots', () => {
            const result = selectServerSlotById('1').projector(initialState);
            expect(result).toBeUndefined();
        });
    });
});
