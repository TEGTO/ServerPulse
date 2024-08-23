import { createReducer, on } from "@ngrx/store";
import { convertToCustomEventStatisticsResponse, convertToServerLoadStatisticsResponse, convertToServerStatisticsResponse, CustomEventStatisticsResponse, ServerLoadStatisticsResponse, ServerStatisticsResponse } from "../../../shared";
import { selectDate, subscribeToCustomStatisticsFailure, subscribeToCustomStatisticsSuccess, subscribeToLoadStatisticsFailure, subscribeToLoadStatisticsSuccess, subscribeToSlotStatisticsFailure, subscribeToSlotStatisticsSuccess } from "../../index";

export interface SlotStatisticsState {
    lastStatistics: { key: string; statistics: ServerStatisticsResponse; } | null,
    error: any
}
const initialSlotStatisticsState: SlotStatisticsState = {
    lastStatistics: null,
    error: null
};
export const slotStatisticsReducer = createReducer(
    initialSlotStatisticsState,

    on(subscribeToSlotStatisticsSuccess, (state, { lastStatistics }) => {
        const statistics = convertToServerStatisticsResponse(JSON.parse(lastStatistics.data));
        const key = lastStatistics.key;
        return {
            ...state,
            lastStatistics: { key: key, statistics: statistics },
            error: null,
        };
    }),
    on(subscribeToSlotStatisticsFailure, (state, { error }) => ({
        ...state,
        error,
    }))
);

export interface SlotLoadStatisticsState {
    currentDate: Date,
    lastLoadStatistics: { key: string; statistics: ServerLoadStatisticsResponse; } | null,
    error: any
}
const initialSlotLoadStatisticsState: SlotLoadStatisticsState = {
    currentDate: new Date(),
    lastLoadStatistics: null,
    error: null
};
export const slotLoadStatisticsReducer = createReducer(
    initialSlotLoadStatisticsState,

    on(selectDate, (state, { date }) => ({
        ...state,
        currentDate: date,
    })),
    on(subscribeToLoadStatisticsSuccess, (state, { lastLoadStatistics }) => {
        const statistics = convertToServerLoadStatisticsResponse(JSON.parse(lastLoadStatistics.data));
        const key = lastLoadStatistics.key;
        return {
            ...state,
            lastLoadStatistics: { key: key, statistics: statistics },
            error: null,
        };
    }),
    on(subscribeToLoadStatisticsFailure, (state, { error }) => ({
        ...state,
        error,
    }))
);

export interface SlotCustomStatisticsState {
    lastStatistics: { key: string; statistics: CustomEventStatisticsResponse; } | null,
    error: any
}
const initialSlotCustomStatisticsState: SlotCustomStatisticsState = {
    lastStatistics: null,
    error: null
};
export const slotCustomStatisticsReducer = createReducer(
    initialSlotCustomStatisticsState,

    on(subscribeToCustomStatisticsSuccess, (state, { lastStatistics }) => {
        const statistics = convertToCustomEventStatisticsResponse(JSON.parse(lastStatistics.data));
        const key = lastStatistics.key;
        return {
            ...state,
            lastStatistics: { key: key, statistics: statistics },
            error: null,
        };
    }),
    on(subscribeToCustomStatisticsFailure, (state, { error }) => ({
        ...state,
        error,
    }))
);
