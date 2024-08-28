import { BaseEventResponse, convertBaseEventResponse, TimeSpan } from "../../../../../index";

export interface LoadEventResponse extends BaseEventResponse {
    endpoint: string;
    method: string;
    statusCode: number;
    duration: TimeSpan;
    timestampUTC: Date;
}
export function convertToServerLoadResponse(data: any): LoadEventResponse {
    const baseResponse = convertBaseEventResponse<LoadEventResponse>(data);
    return {
        ...baseResponse,
        endpoint: data?.Endpoint,
        method: data?.Method,
        statusCode: data?.StatusCode,
        duration: data?.Duration,
        timestampUTC: new Date(data?.TimestampUTC)
    };
}