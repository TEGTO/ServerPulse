import { TimeSpan } from "../../..";

export interface ServerStatisticsResponse {
    isAlive: boolean;
    dataExists: boolean;
    serverLastStartDateTimeUTC: Date | null;
    serverUptime: TimeSpan | null;
    lastServerUptime: TimeSpan | null;
    lastPulseDateTimeUTC: Date | null;
    collectedDateUTC: Date,
    isInitial: boolean;
}

export function convertToServerStatisticsResponse(data: any): ServerStatisticsResponse {
    return {
        isAlive: data?.IsAlive,
        dataExists: data?.DataExists,
        serverLastStartDateTimeUTC: data.ServerLastStartDateTimeUTC ? new Date(data.ServerLastStartDateTimeUTC) : null,
        serverUptime: data.ServerUptime ? TimeSpan.fromString(data.ServerUptime) : null,
        lastServerUptime: data.LastServerUptime ? TimeSpan.fromString(data.LastServerUptime) : null,
        lastPulseDateTimeUTC: data.LastPulseDateTimeUTC ? new Date(data.LastPulseDateTimeUTC) : null,
        collectedDateUTC: data?.CollectedDateUTC,
        isInitial: data?.IsInitial
    };
}