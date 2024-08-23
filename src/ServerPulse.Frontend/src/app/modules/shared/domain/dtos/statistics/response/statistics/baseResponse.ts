export interface BaseStatisticsResponse {
    collectedDateUTC: Date;
    isInitial: boolean;
}

export function convertBaseResponse(data: any): BaseStatisticsResponse {
    return {
        collectedDateUTC: new Date(data?.CollectedDateUTC),
        isInitial: data?.IsInitial ?? false
    };
}