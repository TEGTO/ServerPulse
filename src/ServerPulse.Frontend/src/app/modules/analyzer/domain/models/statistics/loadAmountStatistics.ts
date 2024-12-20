import { BaseStatistics } from "../../..";

export interface LoadAmountStatistics extends BaseStatistics {
    amountOfEvents: number;
    dateFrom: Date;
    dateTo: Date;
}