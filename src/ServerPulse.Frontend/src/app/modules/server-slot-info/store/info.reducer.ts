import { createReducer, on } from "@ngrx/store";
import { addNewLoadEvent, getDailyLoadAmountStatisticsSuccess, getLoadAmountStatisticsInRangeSuccess, getSomeLoadEventsSuccess, setReadFromDate, setSelectedDate } from "..";
import { LoadAmountStatistics, LoadEvent } from "../../analyzer";

export interface SlotInfoState {
    selectedDate: Date,
    readFromDate: Date,
    loadEvents: LoadEvent[],
    loadAmountStatistics: LoadAmountStatistics[],
    secondaryLoadAmountStatistics: LoadAmountStatistics[]
}
const initialSlotInfoState: SlotInfoState = {
    selectedDate: new Date(),
    readFromDate: new Date(),
    loadEvents: [],
    loadAmountStatistics: [],
    secondaryLoadAmountStatistics: []
};
export const slotInfoStateReducer = createReducer(
    initialSlotInfoState,

    on(setSelectedDate, (state, { date }) => ({
        ...state,
        selectedDate: date,
        loadEvents: []
    })),

    on(setReadFromDate, (state, { date }) => ({
        ...state,
        readFromDate: date
    })),

    on(getSomeLoadEventsSuccess, (state, { events }) => {
        const uniqueKeys = new Set(state.loadEvents.map(event => event.id));
        const filteredNewEvents = events.filter(event => !uniqueKeys.has(event.id));

        return {
            ...state,
            loadEvents: [...state.loadEvents, ...filteredNewEvents]
        }
    }),

    on(getDailyLoadAmountStatisticsSuccess, (state, { statistics }) => ({
        ...state,
        loadAmountStatistics: statistics,
    })),

    on(getLoadAmountStatisticsInRangeSuccess, (state, { statistics }) => ({
        ...state,
        secondaryLoadAmountStatistics: statistics,
    })),

    on(addNewLoadEvent, (state, { event }) => {
        const uniqueKeys = new Set(state.loadEvents.map(event => event.id));

        if (uniqueKeys.has(event.id)) {
            return {
                ...state,
            };
        }

        return {
            ...state,
            loadEvents: [event, ...state.loadEvents],
        };
    }),
);