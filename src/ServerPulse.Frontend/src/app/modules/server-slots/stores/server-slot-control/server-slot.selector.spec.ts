import { ServerSlotState } from "./server-slot.reducer";
import { selectServerSlots, selectServerSlotsErrors, selectServerSlotState } from "./server-slot.selector";

describe('Server Slot Selectors', () => {
    const initialState: ServerSlotState = {
        serverSlots: [
            { id: "1", userEmail: "1", name: "1", slotKey: "1" },
            { id: "2", userEmail: "2", name: "2", slotKey: "2" },
        ],
        error: null
    };
    const errorState: ServerSlotState = {
        serverSlots: [],
        error: 'An error occurred'
    };

    it('should select the slot state', () => {
        const result = selectServerSlotState.projector(initialState);
        expect(result).toEqual(initialState);
    });
    it('should select server slots', () => {
        const result = selectServerSlots.projector(initialState);
        expect(result).toEqual(initialState.serverSlots);
    });
    it('should select server slot error', () => {
        const result = selectServerSlotsErrors.projector(errorState);
        expect(result).toEqual(errorState.error);
    });
});