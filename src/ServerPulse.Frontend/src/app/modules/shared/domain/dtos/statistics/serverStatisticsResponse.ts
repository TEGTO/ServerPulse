import { TimeSpan } from "../../..";

export interface ServerStatisticsResponse {
    isAlive: boolean;
    dataExists: boolean;
    serverLastStartDateTime: Date | null;
    serverUptime: TimeSpan | null;
    lastServerUptime: TimeSpan | null;
    lastPulseDateTime: Date | null;
    collectedDateUTC: Date,
}

export function convertToServerStatisticsResponse(data: any): ServerStatisticsResponse {
    return {
        isAlive: data?.IsAlive,
        dataExists: data?.DataExists,
        serverLastStartDateTime: data.ServerLastStartDateTime ? new Date(data.ServerLastStartDateTime) : null,
        serverUptime: data.ServerUptime ? TimeSpan.fromString(data.ServerUptime) : null,
        lastServerUptime: data.LastServerUptime ? TimeSpan.fromString(data.LastServerUptime) : null,
        lastPulseDateTime: data.LastPulseDateTime ? new Date(data.LastPulseDateTime) : null,
        collectedDateUTC: data?.CollectedDateUTC,
    };
}