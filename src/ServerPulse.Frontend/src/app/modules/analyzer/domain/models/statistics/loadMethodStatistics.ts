import { BaseStatistics } from "../../..";

export interface LoadMethodStatistics extends BaseStatistics {
    getAmount: number;
    postAmount: number;
    putAmount: number;
    patchAmount: number;
    deleteAmount: number;
}