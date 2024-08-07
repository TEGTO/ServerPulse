import { convertToServerLoadResponse, ServerLoadResponse } from "../../..";

export interface ServerLoadStatisticsResponse {
    amountOfEvents: number;
    lastEvent: ServerLoadResponse;
}

export function convertToServerLoadStatisticsResponse(data: any): ServerLoadStatisticsResponse {
    return {
        amountOfEvents: data.AmountOfEvents,
        lastEvent: convertToServerLoadResponse(data.LastEvent),
    };
}