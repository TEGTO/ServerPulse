import { createFeatureSelector, createSelector } from "@ngrx/store";
import { LoadAmountStatisticsState, ServerCustomStatisticsState, ServerLifecycleStatisticsState, ServerLoadStatisticsState } from "..";

//#region Lifecycle Statistics

export const selectServerLifecycleStatisticsState = createFeatureSelector<ServerLifecycleStatisticsState>('lifecyclestatistics');

export const selectLifecycleStatisticsByKey = (key: string) => createSelector(
    selectServerLifecycleStatisticsState,
    (state: ServerLifecycleStatisticsState) => state.statisticsMap.get(key) ?? []
);

export const selectLastLifecycleStatisticsByKey = (key: string) => createSelector(
    selectServerLifecycleStatisticsState,
    (state: ServerLifecycleStatisticsState) => {
        const statistics = state.statisticsMap.get(key);
        if (statistics && statistics.length > 0) {
            return statistics[statistics.length - 1] ?? null;
        }
        return null;
    }
);

//#endregion

//#region Load Statistics

export const selectServerLoadStatisticsState = createFeatureSelector<ServerLoadStatisticsState>('loadstatistics');

export const selectLoadStatisticsByKey = (key: string) => createSelector(
    selectServerLoadStatisticsState,
    (state: ServerLoadStatisticsState) => state.statisticsMap.get(key) ?? []
);

export const selectLastLoadEventByKey = (key: string) => createSelector(
    selectServerLoadStatisticsState,
    (state: ServerLoadStatisticsState) => {
        const statistics = state.statisticsMap.get(key);
        if (statistics && statistics.length > 0) {
            return statistics[statistics.length - 1]?.lastEvent ?? null;
        }
        return null;
    }
);
//#endregion

//#region Custom

export const selectServerCustomStatisticsState = createFeatureSelector<ServerCustomStatisticsState>('customstatistics');

export const selectCustomStatisticsByKey = (key: string) => createSelector(
    selectServerCustomStatisticsState,
    (state: ServerCustomStatisticsState) => state.statisticsMap.get(key) ?? []
);

//#endregion

//#region Load Amount Statistics

export const selectLoadAmountStatisticsState = createFeatureSelector<LoadAmountStatisticsState>('loadamountstatistics');

export const selectLoadAmountStatisticsByKey = (key: string) => createSelector(
    selectLoadAmountStatisticsState,
    (state: LoadAmountStatisticsState) => state.statisticsMap.get(key) ?? []
);

//#endregion