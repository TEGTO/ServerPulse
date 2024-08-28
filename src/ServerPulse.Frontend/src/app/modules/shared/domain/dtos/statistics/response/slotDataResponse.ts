import { CustomEventResponse, CustomEventStatisticsResponse, LoadEventResponse, ServerLoadStatisticsResponse, ServerStatisticsResponse } from "../../../../index";

export interface SlotDataResponse {
    collectedDateUTC: Date,
    generalStatistics: ServerStatisticsResponse | null,
    loadStatistics: ServerLoadStatisticsResponse | null,
    customEventStatistics: CustomEventStatisticsResponse | null,
    lastLoadEvents: LoadEventResponse[],
    lastCustomEvents: CustomEventResponse[],
}