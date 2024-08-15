import { TimeSpan } from "../../../../index";

export interface ServerLoadResponse {
    id: string;
    key: string;
    creationDateUTC: Date;
    endpoint: string;
    method: string;
    statusCode: number;
    duration: TimeSpan;
    timestampUTC: Date;
}
export function convertToServerLoadResponse(data: any): ServerLoadResponse {
    return {
        id: data?.Id,
        key: data?.Key,
        creationDateUTC: data?.CreationDateUTC,
        endpoint: data?.Endpoint,
        method: data?.Method,
        statusCode: data?.StatusCode,
        duration: data?.Duration,
        timestampUTC: data?.TimestampUTC,
    };
}