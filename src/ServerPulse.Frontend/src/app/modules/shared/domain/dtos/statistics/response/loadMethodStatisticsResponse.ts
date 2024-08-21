export interface LoadMethodStatisticsResponse {
    getAmount: number;
    postAmount: number;
    putAmount: number;
    patchAmount: number;
    deleteAmount: number;
}
export function convertToSLoadMethodStatisticsResponse(data: any): LoadMethodStatisticsResponse {
    return {
        getAmount: data?.GetAmount,
        postAmount: data?.PostAmount,
        putAmount: data?.PutAmount,
        patchAmount: data?.PatchAmount,
        deleteAmount: data?.DeleteAmount,
    };
}