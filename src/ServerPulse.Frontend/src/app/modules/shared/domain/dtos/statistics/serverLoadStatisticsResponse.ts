import { convertToServerLoadResponse, ServerLoadResponse } from "../../..";

export interface ServerLoadStatisticsResponse {
    amountOfEvents: number;
    lastEvent: ServerLoadResponse | null;
    collectedDateUTC: Date;
    isInitial: boolean;
}

export function convertToServerLoadStatisticsResponse(data: any): ServerLoadStatisticsResponse {
    return {
        amountOfEvents: data?.AmountOfEvents,
        lastEvent: convertToServerLoadResponse(data?.LastEvent),
        collectedDateUTC: data?.CollectedDateUTC,
        isInitial: data?.IsInitial
    };
}