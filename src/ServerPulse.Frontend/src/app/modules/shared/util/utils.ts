import { BaseEventResponse } from "../../analyzer";

/**
* Filters out duplicate items based on `creationDateUTC` and `id`.
*/
export function getUniqueItems(existingItems: BaseEventResponse[], newItems: BaseEventResponse[]): BaseEventResponse[] {
    const existingSet = new Set(existingItems.map(item => `${new Date(item.creationDateUTC).getTime()}-${item.id}`));

    return newItems.filter(item => {
        const creationDate = new Date(item.creationDateUTC);
        const uniqueKey = `${creationDate.getTime()}-${item.id}`;

        if (existingSet.has(uniqueKey)) {
            return false;
        } else {
            existingSet.add(uniqueKey);
            return true;
        }
    });
}