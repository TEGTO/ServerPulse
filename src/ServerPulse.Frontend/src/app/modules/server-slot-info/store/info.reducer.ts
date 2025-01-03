import { createReducer, on } from "@ngrx/store";
import { addNewCustomEvent, addNewLoadEvent, getDailyLoadAmountStatisticsSuccess, getLoadAmountStatisticsInRangeSuccess, getSomeCustomEventsSuccess, getSomeLoadEventsSuccess, setCustomReadFromDate, setReadFromDate, setSelectedDate } from "..";
import { CustomEvent, LoadAmountStatistics, LoadEvent } from "../../analyzer";

export interface SlotInfoState {
    selectedDate: Date,
    readFromDate: Date,
    customReadFromDate: Date,
    loadEvents: LoadEvent[],
    customEvents: CustomEvent[],
    loadAmountStatistics: LoadAmountStatistics[],
    secondaryLoadAmountStatistics: LoadAmountStatistics[]
}
const initialSlotInfoState: SlotInfoState = {
    selectedDate: new Date(),
    readFromDate: new Date(),
    customReadFromDate: new Date(),
    loadEvents: [],
    customEvents: [],
    loadAmountStatistics: [],
    secondaryLoadAmountStatistics: []
};
export const slotInfoStateReducer = createReducer(
    initialSlotInfoState,

    on(setSelectedDate, (state, { date, readFromDate: fromDate }) => ({
        ...state,
        selectedDate: date,
        readFromDate: fromDate,
        loadEvents: []
    })),

    on(setReadFromDate, (state, { date }) => ({
        ...state,
        readFromDate: date
    })),

    on(setCustomReadFromDate, (state, { date }) => ({
        ...state,
        customReadFromDate: date
    })),

    on(getSomeLoadEventsSuccess, (state, { events }) => {
        const allEvents = [...state.loadEvents, ...events];
        const uniqueEventsMap = new Map(allEvents.map(event => [event.id, event]));

        return {
            ...state,
            loadEvents: Array.from(uniqueEventsMap.values()),
        };
    }),

    on(getSomeCustomEventsSuccess, (state, { events }) => {
        const allEvents = [...state.customEvents, ...events];
        const uniqueEventsMap = new Map(allEvents.map(event => [event.id, event]));

        return {
            ...state,
            customEvents: Array.from(uniqueEventsMap.values()),
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

    on(addNewCustomEvent, (state, { event }) => {
        const uniqueKeys = new Set(state.customEvents.map(event => event.id));

        if (uniqueKeys.has(event.id)) {
            return {
                ...state,
            };
        }

        return {
            ...state,
            customEvents: [event, ...state.customEvents],
        };
    }),
);