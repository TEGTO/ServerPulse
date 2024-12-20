import { BaseEvent } from "../../..";

export interface CustomEvent extends BaseEvent {
    name: string;
    description: string;
    serializedMessage: string;
}
