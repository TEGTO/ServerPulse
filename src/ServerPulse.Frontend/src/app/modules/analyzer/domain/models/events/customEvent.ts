import { BaseEvent } from "../../..";

export interface CustomEvent extends BaseEvent {
    name: string;
    description: string;
    serializedMessage: string;
}

export function getDefaultCustomEvent(): CustomEvent {
    return {
        id: "",
        key: "",
        creationDateUTC: new Date(),
        name: "",
        description: "",
        serializedMessage: ""
    }
}
