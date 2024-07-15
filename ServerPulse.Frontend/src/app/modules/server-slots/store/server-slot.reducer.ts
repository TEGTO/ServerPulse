import { createReducer, on } from "@ngrx/store";
import { createServerSlotFailure, createServerSlotSuccess, deleteServerSlotFailure, deleteServerSlotSuccess, getServerSlotsFailure, getServerSlotsSuccess, getServerSlotsWithStringFailure, getServerSlotsWithStringSuccess, updateServerSlotFailure, updateServerSlotSuccess } from "..";
import { applyUpdateRequestOnServerSlot, ServerSlot } from "../../shared";

//ServerSlots
export interface ServerSlotState {
    serverSlots: ServerSlot[],
    error: any
}
const initialServerSlotState: ServerSlotState = {
    serverSlots: [],
    error: null
};
export const serverSlotReducer = createReducer(
    initialServerSlotState,
    //Get Server Slots
    on(getServerSlotsSuccess, (state, { serverSlots: serverSlots }) => ({
        ...state,
        serverSlots: serverSlots,
        error: null
    })),
    on(getServerSlotsFailure, (state, { error: error }) => ({
        ...state,
        serverSlots: [],
        error: error
    })),
    //Get Server Slots with string 
    on(getServerSlotsWithStringSuccess, (state, { serverSlots: serverSlots }) => ({
        ...state,
        serverSlots: serverSlots,
        error: null
    })),
    on(getServerSlotsWithStringFailure, (state, { error: error }) => ({
        ...state,
        serverSlots: [],
        error: error
    })),
    //Create a New Server Slot
    on(createServerSlotSuccess, (state, { serverSlot: serverSlot }) => ({
        ...state,
        serverSlots: [serverSlot, ...state.serverSlots],
        error: null
    })),
    on(createServerSlotFailure, (state, { error: error }) => ({
        ...state,
        serverSlots: [],
        error: error
    })),
    //Update a Server Slot
    on(updateServerSlotSuccess, (state, { updateRequest: updateRequest }) => {
        const index = state.serverSlots.findIndex(s => s.id === updateRequest.id);
        const currentServerSlot = state.serverSlots[index];
        let serverSlot = applyUpdateRequestOnServerSlot(currentServerSlot, updateRequest);
        return {
            ...state,
            serverSlots: state.serverSlots.map(s => s.id === serverSlot.id ? serverSlot : s),
            error: null
        }
    }),
    on(updateServerSlotFailure, (state, { error: error }) => ({
        ...state,
        error: error
    })),
    //Delete a Server Slot
    on(deleteServerSlotSuccess, (state, { id: id }) => ({
        ...state,
        serverSlots: state.serverSlots.filter(x => x.id !== id),
        error: null
    })),
    on(deleteServerSlotFailure, (state, { error: error }) => ({
        ...state,
        error: error
    })),
);