import { BaseStatisticsResponse, CustomEventResponse, mapBaseStatisticsResponseToBaseStatistics, mapCustomEventResponseToCustomEvent, ServerCustomStatistics } from "../../../..";

export interface ServerCustomStatisticsResponse extends BaseStatisticsResponse {
    lastEvent: CustomEventResponse | null;
}

export function mapServerCustomStatisticsResponseToServerCustomStatistics(response: ServerCustomStatisticsResponse): ServerCustomStatistics {
    return {
        ...mapBaseStatisticsResponseToBaseStatistics(response),
        lastEvent: response.lastEvent === null ? null : mapCustomEventResponseToCustomEvent(response.lastEvent)
    };
}