import { BaseStatisticsResponse, LoadMethodStatistics, mapBaseStatisticsResponseToBaseStatistics } from "../../../..";

export interface LoadMethodStatisticsResponse extends BaseStatisticsResponse {
    getAmount: number;
    postAmount: number;
    putAmount: number;
    patchAmount: number;
    deleteAmount: number;
}

export function mapLoadMethodStatisticsResponseToLoadMethodStatistics(response: LoadMethodStatisticsResponse): LoadMethodStatistics {
    return {
        ...mapBaseStatisticsResponseToBaseStatistics(response),
        getAmount: response?.getAmount,
        postAmount: response?.postAmount,
        putAmount: response?.putAmount,
        patchAmount: response?.patchAmount,
        deleteAmount: response?.deleteAmount,
    };
}