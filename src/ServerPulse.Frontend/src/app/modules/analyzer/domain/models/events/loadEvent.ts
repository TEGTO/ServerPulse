import { BaseEvent } from "../../..";
import { TimeSpan } from "../../../../shared";

export interface LoadEvent extends BaseEvent {
    endpoint: string;
    method: string;
    statusCode: number;
    duration: TimeSpan;
    timestampUTC: Date;
}