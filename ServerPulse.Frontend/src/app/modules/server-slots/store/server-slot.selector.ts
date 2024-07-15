import { createFeatureSelector, createSelector } from "@ngrx/store";
import { ServerSlotState } from "..";

//Registration
export const selectServerSlotState = createFeatureSelector<ServerSlotState>('serverslot');
export const selectServerSlots = createSelector(
    selectServerSlotState,
    (state: ServerSlotState) => state.serverSlots
);
export const selectServerSlotsErrors = createSelector(
    selectServerSlotState,
    (state: ServerSlotState) => state.error
);