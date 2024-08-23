import { BaseStatisticsResponse, convertBaseResponse, convertToLoadMethodStatisticsResponse, convertToServerLoadResponse, LoadMethodStatisticsResponse, ServerLoadResponse } from "../../../../../index";

export interface ServerLoadStatisticsResponse extends BaseStatisticsResponse {
    amountOfEvents: number;
    lastEvent: ServerLoadResponse | null;
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