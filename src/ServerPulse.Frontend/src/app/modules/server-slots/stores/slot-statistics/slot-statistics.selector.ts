import { createFeatureSelector, createSelector } from "@ngrx/store";
import { SlotLoadStatisticsState, SlotStatisticsState } from "../..";

//Statistics
export const selectSlotStatisticsState = createFeatureSelector<SlotStatisticsState>('slotstatistics');
export const selectLastStatistics = createSelector(
    selectSlotStatisticsState,
    (state: SlotStatisticsState) => state.lastStatistics
);

//Load Statistics
export const selectSlotLoadStatisticsState = createFeatureSelector<SlotLoadStatisticsState>('slotloadstatistics');
export const selectCurrentDate = createSelector(
    selectSlotLoadStatisticsState,
    (state: SlotLoadStatisticsState) => state.currentDate
);
export const selectLastLoadStatistics = createSelector(
    selectSlotLoadStatisticsState,
    (state: SlotLoadStatisticsState) => state.lastLoadStatistics
);