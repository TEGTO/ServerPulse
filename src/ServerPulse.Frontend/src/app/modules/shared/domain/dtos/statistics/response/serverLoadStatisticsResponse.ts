import { convertToServerLoadResponse, convertToSLoadMethodStatisticsResponse, LoadMethodStatisticsResponse, ServerLoadResponse } from "../../../../index";

export interface ServerLoadStatisticsResponse {
    amountOfEvents: number;
    lastEvent: ServerLoadResponse | null;
    loadMethodStatistics: LoadMethodStatisticsResponse | null;
    collectedDateUTC: Date;
    isInitial: boolean;
}

export function convertToServerLoadStatisticsResponse(data: any): ServerLoadStatisticsResponse {
    return {
        amountOfEvents: data?.AmountOfEvents,
        lastEvent: convertToServerLoadResponse(data?.LastEvent),
        loadMethodStatistics: convertToSLoadMethodStatisticsResponse(data?.LoadMethodStatistics),
        collectedDateUTC: data?.CollectedDateUTC,
        isInitial: data?.IsInitial
    };
}