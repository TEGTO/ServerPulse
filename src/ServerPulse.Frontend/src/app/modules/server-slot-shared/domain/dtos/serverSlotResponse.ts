import { ServerSlot } from "../..";

export interface ServerSlotResponse {
    id: string;
    userEmail: string;
    name: string;
    slotKey: string;
}

export function mapServerSlotResponseToServerSlot(response: ServerSlotResponse): ServerSlot {
    return {
        id: response?.id,
        userEmail: response?.userEmail,
        name: response?.name,
        slotKey: response?.slotKey
    }
}