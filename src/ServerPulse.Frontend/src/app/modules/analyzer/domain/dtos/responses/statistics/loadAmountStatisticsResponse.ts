import { BaseStatisticsResponse, LoadAmountStatistics, mapBaseStatisticsResponseToBaseStatistics } from "../../../..";

export interface LoadAmountStatisticsResponse extends BaseStatisticsResponse {
    amountOfEvents: number;
    dateFrom: Date;
    dateTo: Date;
}

export function mapLoadAmountStatisticsResponseToLoadAmountStatistics(response: LoadAmountStatisticsResponse): LoadAmountStatistics {
    return {
        ...mapBaseStatisticsResponseToBaseStatistics(response),
        amountOfEvents: response?.amountOfEvents,
        dateFrom: new Date(response?.dateFrom),
        dateTo: new Date(response?.dateTo)
    };
}