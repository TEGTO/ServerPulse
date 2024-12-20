import { CustomEventResponse, LoadEventResponse, mapCustomEventResponseToCustomEvent, mapLoadEventResponseToLoadEvent, mapServerCustomStatisticsResponseToServerCustomStatistics, mapServerLifecycleStatisticsResponseToServerLifecycleStatistics, mapServerLoadStatisticsResponseToServerLoadStatistics, ServerCustomStatisticsResponse, ServerLifecycleStatisticsResponse, ServerLoadStatisticsResponse, SlotStatistics } from "../../../..";

export interface SlotStatisticsResponse {
    collectedDateUTC: Date,
    generalStatistics: ServerLifecycleStatisticsResponse | null,
    loadStatistics: ServerLoadStatisticsResponse | null,
    customEventStatistics: ServerCustomStatisticsResponse | null,
    lastLoadEvents: LoadEventResponse[],
    lastCustomEvents: CustomEventResponse[],
}

export function mapSlotStatisticsResponseToSlotStatistics(response: SlotStatisticsResponse): SlotStatistics {
    return {
        collectedDateUTC: response?.collectedDateUTC,
        generalStatistics: response.generalStatistics === null ? null : mapServerLifecycleStatisticsResponseToServerLifecycleStatistics(response.generalStatistics),
        loadStatistics: response.loadStatistics === null ? null : mapServerLoadStatisticsResponseToServerLoadStatistics(response.loadStatistics),
        customEventStatistics: response.customEventStatistics === null ? null : mapServerCustomStatisticsResponseToServerCustomStatistics(response.customEventStatistics),
        lastLoadEvents: response.lastLoadEvents.map(mapLoadEventResponseToLoadEvent),
        lastCustomEvents: response.lastCustomEvents.map(mapCustomEventResponseToCustomEvent),
    };
}