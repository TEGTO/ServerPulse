import { ServerSlot } from "../..";

export interface GetSlotsByEmailResponse {
    id: string;
    userEmail: string;
    name: string;
    slotKey: string;
}

export function mapGetSlotsByEmailResponseToServerSlot(response: GetSlotsByEmailResponse): ServerSlot {
    return {
        id: response?.id,
        userEmail: response?.userEmail,
        name: response?.name,
        slotKey: response?.slotKey
    }
}