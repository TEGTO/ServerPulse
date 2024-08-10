import { createFeatureSelector, createSelector } from "@ngrx/store";
import { SlotLoadStatisticsState } from "../..";

//Registration
export const selectSlotLoadStatisticsState = createFeatureSelector<SlotLoadStatisticsState>('slotloadstatistics');
export const selectCurrentDate = createSelector(
    selectSlotLoadStatisticsState,
    (state: SlotLoadStatisticsState) => state.currentDate
);
