import { BaseEventResponse, convertBaseEventResponse } from "../../../../../index";

export interface CustomEventResponse extends BaseEventResponse {
    name: string;
    description: string;
    serializedMessage: string;
}

export function convertToCustomEventResponse(data: any): CustomEventResponse {
    const baseResponse = convertBaseEventResponse<CustomEventResponse>(data);
    return {
        ...baseResponse,
        name: data?.Name,
        description: data?.Description,
        serializedMessage: data?.SerializedMessage
    };
}