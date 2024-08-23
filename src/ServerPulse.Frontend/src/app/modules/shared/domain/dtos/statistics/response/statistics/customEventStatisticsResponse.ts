import { BaseStatisticsResponse, convertBaseResponse, convertToCustomEventResponse, CustomEventResponse } from "../../../../../index";

export interface CustomEventStatisticsResponse extends BaseStatisticsResponse {
    lastEvent: CustomEventResponse | null;
}
export function convertToCustomEventStatisticsResponse(data: any): CustomEventStatisticsResponse {
    return {
        ...convertBaseResponse(data),
        lastEvent: convertToCustomEventResponse(data?.LastEvent)
    };
}