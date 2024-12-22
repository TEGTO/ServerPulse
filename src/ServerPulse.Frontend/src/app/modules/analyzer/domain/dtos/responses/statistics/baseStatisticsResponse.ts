import { BaseStatistics } from "../../../..";

export interface BaseStatisticsResponse {
    id: string;
    collectedDateUTC: Date;
}

export function mapBaseStatisticsResponseToBaseStatistics(response: BaseStatisticsResponse): BaseStatistics {
    return {
        id: response?.id,
        collectedDateUTC: new Date(response?.collectedDateUTC),
    };
}