import { BaseStatisticsResponse, mapBaseStatisticsResponseToBaseStatistics, ServerLifecycleStatistics } from "../../../..";
import { TimeSpan } from "../../../../../shared";

export interface ServerLifecycleStatisticsResponse extends BaseStatisticsResponse {
    isAlive: boolean;
    dataExists: boolean;
    serverLastStartDateTimeUTC: Date | null;
    serverUptime: string | null;
    lastServerUptime: string | null;
    lastPulseDateTimeUTC: Date | null;
}

export function mapServerLifecycleStatisticsResponseToServerLifecycleStatistics(response: ServerLifecycleStatisticsResponse): ServerLifecycleStatistics {
    return {
        ...mapBaseStatisticsResponseToBaseStatistics(response),
        isAlive: response?.isAlive,
        dataExists: response?.dataExists,
        serverLastStartDateTimeUTC: response.serverLastStartDateTimeUTC === null ? null : new Date(response.serverLastStartDateTimeUTC),
        serverUptime: response.serverUptime === null ? null : TimeSpan.fromString(response.serverUptime),
        lastServerUptime: response.lastServerUptime === null ? null : TimeSpan.fromString(response.lastServerUptime),
        lastPulseDateTimeUTC: response.lastPulseDateTimeUTC === null ? null : new Date(response.lastPulseDateTimeUTC),
    };
}

export function getDefaultServerLifecycleStatisticsResponse(): ServerLifecycleStatisticsResponse {
    return {
        id: "",
        collectedDateUTC: new Date(),
        isAlive: false,
        dataExists: false,
        serverLastStartDateTimeUTC: null,
        serverUptime: null,
        lastServerUptime: null,
        lastPulseDateTimeUTC: null,
    };
}