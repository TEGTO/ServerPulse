import { BaseEventResponse, CustomEvent, mapBaseEventResponseToBaseEvent } from "../../../..";

export interface CustomEventResponse extends BaseEventResponse {
    name: string;
    description: string;
    serializedMessage: string;
}

export function mapCustomEventResponseToCustomEvent(response: CustomEventResponse): CustomEvent {
    return {
        ...mapBaseEventResponseToBaseEvent(response),
        name: response?.name,
        description: response?.description,
        serializedMessage: response?.serializedMessage
    };
}