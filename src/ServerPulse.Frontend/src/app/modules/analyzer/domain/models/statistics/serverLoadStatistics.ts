import { BaseStatistics, LoadEvent, LoadMethodStatistics } from "../../..";

export interface ServerLoadStatistics extends BaseStatistics {
    amountOfEvents: number;
    lastEvent: LoadEvent | null;
    loadMethodStatistics: LoadMethodStatistics | null;
}