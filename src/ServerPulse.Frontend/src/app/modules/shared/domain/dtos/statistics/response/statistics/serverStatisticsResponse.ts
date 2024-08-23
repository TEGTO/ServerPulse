import { BaseStatisticsResponse, convertBaseResponse, parseDate, parseTimeSpan, TimeSpan } from "../../../../../index";

export interface ServerStatisticsResponse extends BaseStatisticsResponse {
    isAlive: boolean;
    dataExists: boolean;
    serverLastStartDateTimeUTC: Date | null;
    serverUptime: TimeSpan | null;
    lastServerUptime: TimeSpan | null;
    lastPulseDateTimeUTC: Date | null;
}

export function convertToServerStatisticsResponse(data: any): ServerStatisticsResponse {
    return {
        ...convertBaseResponse(data),
        isAlive: data?.IsAlive ?? false,
        dataExists: data?.DataExists ?? false,
        serverLastStartDateTimeUTC: parseDate(data?.ServerLastStartDateTimeUTC),
        serverUptime: parseTimeSpan(data?.ServerUptime),
        lastServerUptime: parseTimeSpan(data?.LastServerUptime),
        lastPulseDateTimeUTC: parseDate(data?.LastPulseDateTimeUTC)
    };
}