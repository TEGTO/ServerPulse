import { createReducer, on } from "@ngrx/store";
import { selectDate } from "./slot-statistics.actions";

export interface SlotLoadStatisticsState {
    currentDate: Date
}
const initialSlotLoadStatisticsState: SlotLoadStatisticsState = {
    currentDate: new Date(),
};
export const slotLoadStatisticsReducer = createReducer(
    initialSlotLoadStatisticsState,
    //Select load statistics date
    on(selectDate, (state, { date: date }) => ({
        ...state,
        currentDate: date
    })),
);