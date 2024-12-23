import { createReducer, on } from "@ngrx/store";
import { addLoadEventToLoadAmountStatistics, getLoadAmountStatisticsInRangeSuccess, LoadAmountStatistics, receiveCustomStatisticsSuccess, receiveLifecycleStatisticsSuccess, receiveLoadStatisticsSuccess, ServerCustomStatistics, ServerLifecycleStatistics, ServerLoadStatistics } from "..";
import { TimeSpan } from "../../shared";

//#region Lifecycle Statistics

export interface ServerLifecycleStatisticsState {
    statisticsMap: Map<string, ServerLifecycleStatistics[]>
}
const initialServerLifecycleStatisticsState: ServerLifecycleStatisticsState = {
    statisticsMap: new Map<string, ServerLifecycleStatistics[]>()
};

export const serverLifecycleStatisticsReducer = createReducer(
    initialServerLifecycleStatisticsState,

    on(receiveLifecycleStatisticsSuccess, (state, { key, statistics }) => {
        let updatedStatisticsMap = state.statisticsMap;

        const currentStatistics = updatedStatisticsMap.get(key) ?? [];

        const isDuplicate = currentStatistics.some(x => x.id === statistics.id);

        if (!isDuplicate) {
            updatedStatisticsMap = updatedStatisticsMap.set(key, [...currentStatistics, statistics]);
        }

        return {
            ...state,
            statisticsMap: updatedStatisticsMap.set(key, [...updatedStatisticsMap.get(key) ?? [], statistics])
        };
    }),
);

//#endregion

//#region Load Statistics

export interface ServerLoadStatisticsState {
    statisticsMap: Map<string, ServerLoadStatistics[]>
}
const initialServerLoadStatisticsState: ServerLoadStatisticsState = {
    statisticsMap: new Map<string, ServerLoadStatistics[]>()
};

export const serverLoadStatisticsReducer = createReducer(
    initialServerLoadStatisticsState,

    on(receiveLoadStatisticsSuccess, (state, { key, statistics }) => {
        let updatedStatisticsMap = state.statisticsMap;

        const currentStatistics = updatedStatisticsMap.get(key) ?? [];

        const isDuplicate = currentStatistics.some(x => x.id === statistics.id);

        if (!isDuplicate) {
            updatedStatisticsMap = updatedStatisticsMap.set(key, [...currentStatistics, statistics]);
        }

        return {
            ...state,
            statisticsMap: updatedStatisticsMap
        };
    }),
);

//#endregion

//#region Custom Statistics

export interface ServerCustomStatisticsState {
    statisticsMap: Map<string, ServerCustomStatistics[]>
}
const initialServerCustomStatisticsState: ServerCustomStatisticsState = {
    statisticsMap: new Map<string, ServerCustomStatistics[]>()
};

export const serverCustomStatisticsReducer = createReducer(
    initialServerCustomStatisticsState,

    on(receiveCustomStatisticsSuccess, (state, { key, statistics }) => {
        let updatedStatisticsMap = state.statisticsMap;

        const currentStatistics = updatedStatisticsMap.get(key) ?? [];

        const isDuplicate = currentStatistics.some(x => x.id === statistics.id);

        if (!isDuplicate) {
            updatedStatisticsMap = updatedStatisticsMap.set(key, [...currentStatistics, statistics]);
        }

        return {
            ...state,
            statisticsMap: updatedStatisticsMap.set(key, [...updatedStatisticsMap.get(key) ?? [], statistics])
        };
    }),
);

//#endregion

//#region Load Amount Statistics

export interface LoadAmountStatisticsState {
    statisticsMap: Map<string, LoadAmountStatistics[]>,
    timespanMap: Map<string, TimeSpan>,
    addedEvents: Map<string, string>,
}
const initialLoadAmountStatisticsState: LoadAmountStatisticsState = {
    statisticsMap: new Map<string, LoadAmountStatistics[]>(),
    timespanMap: new Map<string, TimeSpan>(),
    addedEvents: new Map<string, string>(),
};

export const serverLoadAmountStatisticsReducer = createReducer(
    initialLoadAmountStatisticsState,

    on(getLoadAmountStatisticsInRangeSuccess, (state, { key, statistics, timespan }) => {
        const updatedStatisticsMap = state.statisticsMap
        const updatedTimespanMap = state.timespanMap
        return {
            ...state,
            statisticsMap: updatedStatisticsMap.set(key, statistics),
            timespanMap: updatedTimespanMap.set(key, timespan)
        };
    }),

    on(addLoadEventToLoadAmountStatistics, (state, { key, event }) => {
        const updatedStatisticsMap = state.statisticsMap
        const updatedTimespanMap = state.timespanMap
        const updatedAddedEvents = state.addedEvents

        let isPlaceFound = false;
        let statistics = updatedStatisticsMap.get(key) ?? [];

        if (!updatedAddedEvents.has(key) || updatedAddedEvents.get(key) !== event.id) {
            statistics = statistics.map(s => {
                if (s.dateFrom <= event.creationDateUTC && s.dateTo >= event.creationDateUTC) {
                    isPlaceFound = true;
                    return {
                        ...s,
                        amountOfEvents: s.amountOfEvents + 1
                    };
                }
                return s;
            });

            if (!isPlaceFound && updatedTimespanMap.has(key)) {
                const timespanMilliseconds = updatedTimespanMap.get(key)!.toMilliseconds;
                const dateTo = new Date(event.creationDateUTC.getTime() + timespanMilliseconds);

                statistics.push({
                    id: `${key}_${event.creationDateUTC.getTime()}`,
                    collectedDateUTC: new Date(),
                    amountOfEvents: 1,
                    dateFrom: event.creationDateUTC,
                    dateTo: dateTo,
                });
            }

            updatedAddedEvents.set(key, event.id);
        }

        return {
            ...state,
            statisticsMap: updatedStatisticsMap.set(key, statistics),
            addedEvents: updatedAddedEvents
        };
    }),
);

//#endregion