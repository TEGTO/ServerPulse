import { TimeSpan } from "../../..";

export interface ServerLoadResponse {
    id: string;
    creationDateUTC: Date;
    endpoint: string;
    method: string;
    statusCode: number;
    duration: TimeSpan;
    timestampUTC: Date;
}
export function convertToServerLoadResponse(data: any): ServerLoadResponse {
    return {
        id: data.Id,
        creationDateUTC: data.CreationDateUTC,
        endpoint: data.Endpoint,
        method: data.Method,
        statusCode: data.StatusCode,
        duration: data.Duration,
        timestampUTC: data.TimestampUTC,
    };
}