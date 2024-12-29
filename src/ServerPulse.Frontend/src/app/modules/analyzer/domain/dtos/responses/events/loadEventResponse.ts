import { BaseEventResponse, LoadEvent, mapBaseEventResponseToBaseEvent } from "../../../..";
import { TimeSpan } from "../../../../../shared";

export interface LoadEventResponse extends BaseEventResponse {
    endpoint: string;
    method: string;
    statusCode: number;
    duration: string;
    timestampUTC: Date;
}

export function mapLoadEventResponseToLoadEvent(response: LoadEventResponse): LoadEvent {
    return {
        ...mapBaseEventResponseToBaseEvent(response),
        endpoint: response?.endpoint,
        method: response?.method,
        statusCode: response?.statusCode,
        duration: TimeSpan.fromString(response?.duration),
        timestampUTC: new Date(response?.timestampUTC)
    };
}