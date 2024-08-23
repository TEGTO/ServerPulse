export interface BaseEventResponse {
    id: string;
    key: string;
    creationDateUTC: Date;
}
export function convertBaseEventResponse<T extends BaseEventResponse>(data: any): T {
    return {
        id: data?.Id,
        key: data?.Key,
        creationDateUTC: new Date(data?.CreationDateUTC)
    } as T;
}