import { ServerSlotResponse, UpdateServerSlotRequest } from "../../..";

export interface ServerSlot {
    id: string;
    userEmail: string;
    name: string;
    slotKey: string;
}

export function serverSlotResponseToServerSlot(response: ServerSlotResponse): ServerSlot {
    let serverSlot: ServerSlot = {
        id: response.id,
        userEmail: response.userEmail,
        name: response.name,
        slotKey: response.slotKey
    }
    return serverSlot;
}

export function applyUpdateRequestOnServerSlot(serverSlot: ServerSlot, request: UpdateServerSlotRequest): ServerSlot {
    let newServerSlot: ServerSlot = {
        id: serverSlot.id,
        userEmail: serverSlot.userEmail,
        name: request.name,
        slotKey: serverSlot.slotKey
    }
    return newServerSlot;
}
