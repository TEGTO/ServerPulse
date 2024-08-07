import { TimeSpan } from "../../..";

export interface ServerLoadResponse {
    id: string;
    endpoint: string;
    method: string;
    statusCode: number;
    duration: TimeSpan;
    timestampUTC: Date;
}
export function convertToServerLoadResponse(data: any): ServerLoadResponse {
    return {
        id: data.Id,
        endpoint: data.Endpoint,
        method: data.Method,
        statusCode: data.StatusCode,
        duration: data.Duration,
        timestampUTC: data.TimestampUTC,
    };
}