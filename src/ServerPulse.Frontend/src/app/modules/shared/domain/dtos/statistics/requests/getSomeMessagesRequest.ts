export interface GetSomeMessagesRequest {
    key: string;
    numberOfMessages: number;
    startDate: Date;
    readNew: boolean;
}