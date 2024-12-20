import { BaseStatistics, CustomEvent } from "../../..";

export interface ServerCustomStatistics extends BaseStatistics {
    lastEvent: CustomEvent | null;
}
