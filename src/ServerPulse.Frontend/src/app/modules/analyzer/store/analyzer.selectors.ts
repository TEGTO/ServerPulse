import { createFeatureSelector, createSelector } from "@ngrx/store";
import { LoadAmountStatisticsState, ServerCustomStatisticsState, ServerLifecycleStatisticsState, ServerLoadStatisticsState } from "..";

//#region Lifecycle Statistics

export const selectServerLifecycleStatisticsState = createFeatureSelector<ServerLifecycleStatisticsState>('lifecyclestatistics');

export const selectLifecycleStatisticsByKey = (key: string) => createSelector(
    selectServerLifecycleStatisticsState,
    (state: ServerLifecycleStatisticsState) => state.statistics.get(key) ?? []
);

//#endregion

//#region Load Statistics

export const selectServerLoadStatisticsState = createFeatureSelector<ServerLoadStatisticsState>('loadstatistics');

export const selectLoadStatisticsByKey = (key: string) => createSelector(
    selectServerLoadStatisticsState,
    (state: ServerLoadStatisticsState) => state.statistics.get(key) ?? []
);

export const selectLastLoadEventByKey = (key: string) => createSelector(
    selectServerLoadStatisticsState,
    (state: ServerLoadStatisticsState) => state.statistics.get(key)?.pop()?.lastEvent ?? null
);

//#endregion

//#region Custom

export const selectServerCustomStatisticsState = createFeatureSelector<ServerCustomStatisticsState>('customstatistics');

export const selectCustomStatisticsByKey = (key: string) => createSelector(
    selectServerCustomStatisticsState,
    (state: ServerCustomStatisticsState) => state.statistics.get(key) ?? []
);

//#endregion

//#region Load Amount Statistics

export const selectLoadAmountStatisticsState = createFeatureSelector<LoadAmountStatisticsState>('loadamountstatistics');

export const selectLoadAmountStatisticsByKey = (key: string) => createSelector(
    selectLoadAmountStatisticsState,
    (state: LoadAmountStatisticsState) => state.statistics.get(key) ?? []
);

//#endregion