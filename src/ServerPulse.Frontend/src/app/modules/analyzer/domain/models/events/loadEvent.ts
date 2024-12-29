import { BaseEvent } from "../../..";
import { TimeSpan } from "../../../../shared";

export interface LoadEvent extends BaseEvent {
    endpoint: string;
    method: string;
    statusCode: number;
    duration: TimeSpan;
    timestampUTC: Date;
}

export function getDefaultLoadEvent(): LoadEvent {
    return {
        id: "",
        key: "",
        creationDateUTC: new Date(),
        endpoint: "",
        method: "",
        statusCode: 0,
        duration: new TimeSpan(0, 0, 0, 0),
        timestampUTC: new Date()
    }
}