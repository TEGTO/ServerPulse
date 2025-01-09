import { createReducer, on } from "@ngrx/store";
import { addNewCustomEvent, addNewLoadEvent, getDailyLoadAmountStatisticsSuccess, getLoadAmountStatisticsInRangeSuccess, getSomeCustomEventsSuccess, getSomeLoadEventsSuccess, setCustomReadFromDate, setLoadStatisticsInterval, setReadFromDate, setSecondaryLoadStatisticsInterval, setSelectedDate } from "..";
import { CustomEvent, LoadAmountStatistics, LoadEvent } from "../../analyzer";

export interface SlotInfoState {
    selectedDate: Date,
    readFromDate: Date,
    customReadFromDate: Date,
    loadStatisticsInterval: number,
    secondaryLoadStatisticsInterval: number,
    loadEvents: LoadEvent[],
    customEvents: CustomEvent[],
    loadAmountStatistics: LoadAmountStatistics[],
    secondaryLoadAmountStatistics: LoadAmountStatistics[]
}
const initialSlotInfoState: SlotInfoState = {
    selectedDate: new Date(),
    readFromDate: new Date(),
    customReadFromDate: new Date(),
    loadStatisticsInterval: 0,
    secondaryLoadStatisticsInterval: 0,
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

    on(setLoadStatisticsInterval, (state, { interval }) => ({
        ...state,
        loadStatisticsInterval: interval
    })),

    on(setSecondaryLoadStatisticsInterval, (state, { interval }) => ({
        ...state,
        secondaryLoadStatisticsInterval: interval
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

        const statistics = addEventToLoadAmountStatistics(event, state.loadAmountStatistics, state.loadStatisticsInterval);
        const secondaryStatistics = addEventToLoadAmountStatistics(event, state.secondaryLoadAmountStatistics, state.secondaryLoadStatisticsInterval);

        return {
            ...state,
            loadEvents: [event, ...state.loadEvents],
            loadAmountStatistics: statistics,
            secondaryLoadAmountStatistics: secondaryStatistics,
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

function addEventToLoadAmountStatistics(
    loadEvent: LoadEvent,
    statistics: LoadAmountStatistics[],
    intervalTime: number
): LoadAmountStatistics[] {
    const eventTime = new Date(loadEvent.timestampUTC).getTime();

    let isUpdated = false;

    const updatedStatistics = statistics.map(stat => {
        if (stat.dateFrom.getTime() <= eventTime && stat.dateTo.getTime() >= eventTime) {
            isUpdated = true;
            return {
                ...stat,
                amountOfEvents: stat.amountOfEvents + 1,
            };
        }
        return stat;
    });

    if (!isUpdated) {
        updatedStatistics.push({
            id: crypto.randomUUID(),
            collectedDateUTC: new Date(),
            dateFrom: new Date(),
            dateTo: new Date(new Date().getTime() + intervalTime),
            amountOfEvents: 1,
        });
    }

    return updatedStatistics;
}
