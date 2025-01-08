import { ServerSlot } from "../..";

export interface CreateSlotResponse {
    id: string;
    userEmail: string;
    name: string;
    slotKey: string;
}

export function mapCreateSlotResponseToServerSlot(response: CreateSlotResponse): ServerSlot {
    return {
        id: response?.id,
        userEmail: response?.userEmail,
        name: response?.name,
        slotKey: response?.slotKey
    }
}