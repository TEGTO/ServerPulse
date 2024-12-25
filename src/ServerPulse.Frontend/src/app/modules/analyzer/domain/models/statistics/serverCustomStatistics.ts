import { BaseStatistics, CustomEvent } from "../../..";

export interface ServerCustomStatistics extends BaseStatistics {
    lastEvent: CustomEvent | null;
}

export function getDefaultServerCustomStatistics(): ServerCustomStatistics {
    return {
        id: "",
        collectedDateUTC: new Date(),
        lastEvent: null,
    }
}