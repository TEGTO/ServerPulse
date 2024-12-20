import { BaseStatisticsResponse, LoadEventResponse, LoadMethodStatisticsResponse, mapBaseStatisticsResponseToBaseStatistics, mapLoadEventResponseToLoadEvent, mapLoadMethodStatisticsResponseToLoadMethodStatistics, ServerLoadStatistics } from "../../../..";

export interface ServerLoadStatisticsResponse extends BaseStatisticsResponse {
    amountOfEvents: number;
    lastEvent: LoadEventResponse | null;
    loadMethodStatistics: LoadMethodStatisticsResponse | null;
}

export function mapServerLoadStatisticsResponseToServerLoadStatistics(response: ServerLoadStatisticsResponse): ServerLoadStatistics {
    return {
        ...mapBaseStatisticsResponseToBaseStatistics(response),
        amountOfEvents: response?.amountOfEvents,
        lastEvent: response.lastEvent === null ? null : mapLoadEventResponseToLoadEvent(response.lastEvent),
        loadMethodStatistics: response.loadMethodStatistics === null ? null : mapLoadMethodStatisticsResponseToLoadMethodStatistics(response.loadMethodStatistics)
    };
}