import { BaseEventResponse, convertBaseEventResponse, TimeSpan } from "../../../../../index";

export interface ServerLoadResponse extends BaseEventResponse {
    endpoint: string;
    method: string;
    statusCode: number;
    duration: TimeSpan;
    timestampUTC: Date;
}
export function convertToServerLoadResponse(data: any): ServerLoadResponse {
    const baseResponse = convertBaseEventResponse<ServerLoadResponse>(data);
    return {
        ...baseResponse,
        endpoint: data?.Endpoint,
        method: data?.Method,
        statusCode: data?.StatusCode,
        duration: data?.Duration,
        timestampUTC: new Date(data?.TimestampUTC)
    };
}