import { BaseStatisticsResponse } from "../../../../../index";

export interface LoadAmountStatisticsResponse extends BaseStatisticsResponse {
    amountOfEvents: number;
    date: Date;
}