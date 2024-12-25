import { BaseStatistics } from "../../..";

export interface LoadAmountStatistics extends BaseStatistics {
    amountOfEvents: number;
    dateFrom: Date;
    dateTo: Date;
}

export function getDefaultLoadAmountStatistics(): LoadAmountStatistics {
    return {
        id: "",
        collectedDateUTC: new Date(),
        amountOfEvents: 0,
        dateFrom: new Date(),
        dateTo: new Date()
    }
}