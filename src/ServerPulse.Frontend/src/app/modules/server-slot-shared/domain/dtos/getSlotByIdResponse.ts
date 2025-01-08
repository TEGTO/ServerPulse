import { ServerSlot } from "../..";

export interface GetSlotByIdResponse {
    id: string;
    userEmail: string;
    name: string;
    slotKey: string;
}

export function mapGetSlotByIdResponseToServerSlot(response: GetSlotByIdResponse): ServerSlot {
    return {
        id: response?.id,
        userEmail: response?.userEmail,
        name: response?.name,
        slotKey: response?.slotKey
    }
}