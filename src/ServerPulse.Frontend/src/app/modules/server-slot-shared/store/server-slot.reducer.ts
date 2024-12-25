import { createReducer, on } from "@ngrx/store";
import { applyUpdateRequestOnServerSlot, createServerSlotSuccess, deleteServerSlotSuccess, getServerSlotByIdSuccess, getUserServerSlotsSuccess, ServerSlot, updateServerSlotSuccess } from "..";

export interface ServerSlotState {
    serverSlots: ServerSlot[],
}
const initialServerSlotState: ServerSlotState = {
    serverSlots: [],
};

export const serverSlotReducer = createReducer(
    initialServerSlotState,

    on(getServerSlotByIdSuccess, (state, { serverSlot }) => {
        const serverSlots = state.serverSlots.filter(x => x.id !== serverSlot.id);
        serverSlots.push(serverSlot);
        return {
            ...state,
            serverSlots: serverSlots,
            error: null
        }
    }),

    on(getUserServerSlotsSuccess, (state, { serverSlots }) => ({
        ...state,
        serverSlots: serverSlots,
    })),

    on(createServerSlotSuccess, (state, { serverSlot }) => ({
        ...state,
        serverSlots: [serverSlot, ...state.serverSlots],
    })),

    on(updateServerSlotSuccess, (state, { req: updateRequest }) => {
        const index = state.serverSlots.findIndex(s => s.id === updateRequest.id);
        const currentServerSlot = state.serverSlots[index];
        const serverSlot = applyUpdateRequestOnServerSlot(currentServerSlot, updateRequest);
        return {
            ...state,
            serverSlots: state.serverSlots.map(s => s.id === serverSlot.id ? serverSlot : s),
        }
    }),

    on(deleteServerSlotSuccess, (state, { id }) => ({
        ...state,
        serverSlots: state.serverSlots.filter(x => x.id !== id),
    })),
);