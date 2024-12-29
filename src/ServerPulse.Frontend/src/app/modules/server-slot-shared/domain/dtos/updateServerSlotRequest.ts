import { ServerSlot } from "../..";

export interface UpdateServerSlotRequest {
    id: string;
    name: string;
}

export function applyUpdateRequestOnServerSlot(slot: ServerSlot, updateRequest: UpdateServerSlotRequest): ServerSlot {
    return {
        ...slot,
        name: updateRequest.name,
    }
}