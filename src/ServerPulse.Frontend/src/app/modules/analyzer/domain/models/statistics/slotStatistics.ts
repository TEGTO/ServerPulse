import { CustomEvent, LoadEvent, ServerCustomStatistics, ServerLifecycleStatistics, ServerLoadStatistics } from "../../..";

export interface SlotStatistics {
    collectedDateUTC: Date,
    generalStatistics: ServerLifecycleStatistics | null,
    loadStatistics: ServerLoadStatistics | null,
    customEventStatistics: ServerCustomStatistics | null,
    lastLoadEvents: LoadEvent[],
    lastCustomEvents: CustomEvent[],
}

export function getDefaultSlotStatistics(): SlotStatistics {
    return {
        collectedDateUTC: new Date(),
        generalStatistics: null,
        loadStatistics: null,
        customEventStatistics: null,
        lastLoadEvents: [],
        lastCustomEvents: [],
    }
}