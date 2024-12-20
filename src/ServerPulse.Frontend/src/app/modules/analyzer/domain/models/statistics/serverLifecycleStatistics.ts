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
