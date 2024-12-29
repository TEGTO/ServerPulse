import { BaseStatistics, LoadEvent, LoadMethodStatistics } from "../../..";

export interface ServerLoadStatistics extends BaseStatistics {
    amountOfEvents: number;
    lastEvent: LoadEvent | null;
    loadMethodStatistics: LoadMethodStatistics | null;
}

export function getDefaultServerLoadStatistics(): ServerLoadStatistics {
    return {
        id: "",
        collectedDateUTC: new Date(),
        amountOfEvents: 0,
        lastEvent: null,
        loadMethodStatistics: null,
    }
}