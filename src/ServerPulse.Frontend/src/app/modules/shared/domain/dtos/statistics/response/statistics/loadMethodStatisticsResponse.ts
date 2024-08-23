import { BaseStatisticsResponse, convertBaseResponse } from "../../../../../index";

export interface LoadMethodStatisticsResponse extends BaseStatisticsResponse {
    getAmount: number;
    postAmount: number;
    putAmount: number;
    patchAmount: number;
    deleteAmount: number;
}
export function convertToLoadMethodStatisticsResponse(data: any): LoadMethodStatisticsResponse {
    return {
        ...convertBaseResponse(data),
        getAmount: data?.GetAmount ?? 0,
        postAmount: data?.PostAmount ?? 0,
        putAmount: data?.PutAmount ?? 0,
        patchAmount: data?.PatchAmount ?? 0,
        deleteAmount: data?.DeleteAmount ?? 0
    };
}