import { createReducer, on } from "@ngrx/store";
import { addLoadEventToLoadAmountStatistics, getLoadAmountStatisticsInRangeSuccess, LoadAmountStatistics, receiveCustomStatisticsSuccess, receiveLifecycleStatisticsSuccess, receiveLoadStatisticsSuccess, ServerCustomStatistics, ServerLifecycleStatistics, ServerLoadStatistics } from "..";
import { TimeSpan } from "../../shared";

//#region Lifecycle Statistics

export interface ServerLifecycleStatisticsState {
    statistics: Map<string, ServerLifecycleStatistics[]>
}
const initialServerLifecycleStatisticsState: ServerLifecycleStatisticsState = {
    statistics: new Map<string, ServerLifecycleStatistics[]>()
};

export const serverLifecycleStatisticsReducer = createReducer(
    initialServerLifecycleStatisticsState,

    on(receiveLifecycleStatisticsSuccess, (state, { key, statistics }) => {
        return {
            ...state,
            statistics: state.statistics.set(key, [...state.statistics.get(key) ?? [], statistics])
        };
    }),
);

//#endregion

//#region Load Statistics

export interface ServerLoadStatisticsState {
    statistics: Map<string, ServerLoadStatistics[]>
}
const initialServerLoadStatisticsState: ServerLoadStatisticsState = {
    statistics: new Map<string, ServerLoadStatistics[]>()
};

export const serverLoadStatisticsReducer = createReducer(
    initialServerLoadStatisticsState,

    on(receiveLoadStatisticsSuccess, (state, { key, statistics }) => {
        return {
            ...state,
            statistics: state.statistics.set(key, [...state.statistics.get(key) ?? [], statistics])
        };
    }),
);

//#endregion

//#region Custom Statistics

export interface ServerCustomStatisticsState {
    statistics: Map<string, ServerCustomStatistics[]>
}
const initialServerCustomStatisticsState: ServerCustomStatisticsState = {
    statistics: new Map<string, ServerCustomStatistics[]>()
};

export const serverCustomStatisticsReducer = createReducer(
    initialServerCustomStatisticsState,

    on(receiveCustomStatisticsSuccess, (state, { key, statistics }) => {
        return {
            ...state,
            statistics: state.statistics.set(key, [...state.statistics.get(key) ?? [], statistics])
        };
    }),
);

//#endregion

//#region Load Amount Statistics

export interface LoadAmountStatisticsState {
    statistics: Map<string, LoadAmountStatistics[]>,
    timespans: Map<string, TimeSpan>
}
const initialLoadAmountStatisticsState: LoadAmountStatisticsState = {
    statistics: new Map<string, LoadAmountStatistics[]>(),
    timespans: new Map<string, TimeSpan>(),
};

export const serverLoadAmountStatisticsReducer = createReducer(
    initialLoadAmountStatisticsState,

    on(getLoadAmountStatisticsInRangeSuccess, (state, { key, statistics, timespan }) => {
        return {
            ...state,
            statistics: state.statistics.set(key, statistics),
            timespans: state.timespans.set(key, timespan)
        };
    }),

    on(addLoadEventToLoadAmountStatistics, (state, { key, event }) => {

        let isPlaceFound = false;

        let statistics = state.statistics.get(key) ?? [];

        statistics = statistics.map(statistic => {
            if (statistic.dateFrom <= event.creationDateUTC && statistic.dateTo >= event.creationDateUTC) {
                isPlaceFound = true;
                return {
                    ...statistic,
                    amountOfEvents: statistic.amountOfEvents + 1
                };
            }
            return statistic;
        });

        if (!isPlaceFound && state.timespans.has(key)) {
            const timespanMilliseconds = state.timespans.get(key)!.milliseconds;
            const dateTo = new Date(event.creationDateUTC.getTime() + timespanMilliseconds);

            statistics.push({
                collectedDateUTC: new Date(),
                amountOfEvents: 1,
                dateFrom: event.creationDateUTC,
                dateTo: dateTo,
            });
        }

        return {
            ...state,
            statistics: state.statistics.set(key, statistics),
        };
    }),
);

//#endregion
