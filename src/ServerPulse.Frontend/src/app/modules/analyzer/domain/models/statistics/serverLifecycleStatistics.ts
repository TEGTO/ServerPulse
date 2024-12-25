import { BaseStatistics } from "../../..";
import { TimeSpan } from "../../../../shared";

export interface ServerLifecycleStatistics extends BaseStatistics {
    isAlive: boolean;
    dataExists: boolean;
    serverLastStartDateTimeUTC: Date | null;
    serverUptime: TimeSpan | null;
    lastServerUptime: TimeSpan | null;
    lastPulseDateTimeUTC: Date | null;
}

export function getDefaultServerLifecycleStatistics(): ServerLifecycleStatistics {
    return {
        id: "",
        collectedDateUTC: new Date(),
        isAlive: false,
        dataExists: false,
        serverLastStartDateTimeUTC: null,
        serverUptime: null,
        lastServerUptime: null,
        lastPulseDateTimeUTC: null
    }
}