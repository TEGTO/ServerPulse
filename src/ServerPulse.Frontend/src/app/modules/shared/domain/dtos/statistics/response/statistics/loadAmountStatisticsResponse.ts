import { BaseStatisticsResponse } from "../../../../../index";

export interface LoadAmountStatisticsResponse extends BaseStatisticsResponse {
    amountOfEvents: number;
    dateFrom: Date;
    dateTo: Date;
}

export function transformLoadAmountStatisticsResponseDate(response: LoadAmountStatisticsResponse): LoadAmountStatisticsResponse {
    return {
        ...response,
        dateFrom: new Date(response.dateFrom),
        dateTo: new Date(response.dateTo)
    };
}