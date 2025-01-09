import { createFeatureSelector, createSelector } from "@ngrx/store";
import { SlotInfoState } from "..";

export const selectSlotInfoState = createFeatureSelector<SlotInfoState>('slotinfo');

export const selectLoadEvents = createSelector(
    selectSlotInfoState,
    (state: SlotInfoState) => state.loadEvents
);

export const selectCustomEvents = createSelector(
    selectSlotInfoState,
    (state: SlotInfoState) => state.customEvents
);

export const selectSelectedDate = createSelector(
    selectSlotInfoState,
    (state: SlotInfoState) => state.selectedDate
);

export const selectReadFromDate = createSelector(
    selectSlotInfoState,
    (state: SlotInfoState) => state.readFromDate
);

export const selectCustomReadFromDate = createSelector(
    selectSlotInfoState,
    (state: SlotInfoState) => state.customReadFromDate
);

export const selectLoadAmountStatistics = createSelector(
    selectSlotInfoState,
    (state: SlotInfoState) => state.loadAmountStatistics
);

export const selectSecondaryLoadAmountStatistics = createSelector(
    selectSlotInfoState,
    (state: SlotInfoState) => state.secondaryLoadAmountStatistics
);