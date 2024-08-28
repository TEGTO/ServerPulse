import { BaseStatisticsResponse, convertBaseResponse, convertToLoadMethodStatisticsResponse, convertToServerLoadResponse, LoadEventResponse, LoadMethodStatisticsResponse } from "../../../../../index";

export interface ServerLoadStatisticsResponse extends BaseStatisticsResponse {
    amountOfEvents: number;
    lastEvent: LoadEventResponse | null;
    loadMethodStatistics: LoadMethodStatisticsResponse | null;
}

export function convertToServerLoadStatisticsResponse(data: any): ServerLoadStatisticsResponse {
    return {
        ...convertBaseResponse(data),
        amountOfEvents: data?.AmountOfEvents ?? 0,
        lastEvent: convertToServerLoadResponse(data?.LastEvent),
        loadMethodStatistics: convertToLoadMethodStatisticsResponse(data?.LoadMethodStatistics)
    };
}