
import { BaseEvent } from "../../../..";

export interface BaseEventResponse {
    id: string;
    key: string;
    creationDateUTC: Date;
}

export function mapBaseEventResponseToBaseEvent(response: BaseEventResponse): BaseEvent {
    return {
        id: response?.id,
        key: response?.key,
        creationDateUTC: new Date(response?.creationDateUTC)
    };
}