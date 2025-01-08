import { ServerSlot } from "../..";

export interface UpdateSlotRequest {
    id: string;
    name: string;
}

export function applyUpdateRequestOnServerSlot(slot: ServerSlot, updateRequest: UpdateSlotRequest): ServerSlot {
    return {
        ...slot,
        name: updateRequest.name,
    }
}