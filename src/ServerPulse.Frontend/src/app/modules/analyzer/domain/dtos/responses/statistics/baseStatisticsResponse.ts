import { BaseStatistics } from "../../../..";

export interface BaseStatisticsResponse {
    collectedDateUTC: Date;
}

export function mapBaseStatisticsResponseToBaseStatistics(response: BaseStatisticsResponse): BaseStatistics {
    return {
        collectedDateUTC: new Date(response?.collectedDateUTC),
    };
}