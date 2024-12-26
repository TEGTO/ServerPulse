/* eslint-disable @typescript-eslint/no-explicit-any */
import { createAction, props } from "@ngrx/store";
import { LoadAmountStatisticsResponse, LoadEvent, MessageAmountInRangeRequest, ServerCustomStatistics, ServerLifecycleStatistics, ServerLoadStatistics } from "..";
import { TimeSpan } from "../../shared";

//#region Lifecycle Statistics

export const startLifecycleStatisticsReceiving = createAction(
    '[Statistics] Start Lifecycle Statistics Receiving ',
    props<{ key: string, getInitial?: boolean }>()
);
export const stopLifecycleStatisticsReceiving = createAction(
    '[Statistics] Stop Lifecycle Statistics Receiving',
    props<{ key: string }>()
);

export const receiveLifecycleStatisticsSuccess = createAction(
    '[Statistics] Receive Lifecycle Statistics Success',
    props<{ key: string, statistics: ServerLifecycleStatistics }>()
);
export const receiveLifecycleStatisticsFailure = createAction(
    '[Chat] Receive Lifecycle Statistics Failure',
    props<{ error: any }>()
);

//#endregion

//#region Load Statistics

export const startLoadStatisticsReceiving = createAction(
    '[Statistics] Start Load Statistics Receiving ',
    props<{ key: string, getInitial?: boolean }>()
);
export const stopLoadKeyListening = createAction(
    '[Statistics] Stop Load Key Listening',
    props<{ key: string }>()
);

export const receiveLoadStatisticsSuccess = createAction(
    '[Statistics] Receive Load Statistics Success',
    props<{ key: string, statistics: ServerLoadStatistics }>()
);
export const receiveLoadStatisticsFailure = createAction(
    '[Chat] Receive Load Statistics Failure',
    props<{ error: any }>()
);

//#endregion

//#region Custom Statistics

export const startCustomStatisticsReceiving = createAction(
    '[Statistics] Start Custom Statistics Receiving ',
    props<{ key: string, getInitial?: boolean }>()
);
export const stopCustomStatisticsReceiving = createAction(
    '[Statistics] Stop Custom Statistics Receiving',
    props<{ key: string }>()
);

export const receiveCustomStatisticsSuccess = createAction(
    '[Statistics] Receive Custom Statistics Success',
    props<{ key: string, statistics: ServerCustomStatistics }>()
);
export const receiveCustomStatisticsFailure = createAction(
    '[Chat] Receive Custom Statistics Failure',
    props<{ error: any }>()
);

//#endregion

//#region Load Amount Statistics

export const getLoadAmountStatisticsInRange = createAction(
    '[Statistics] Get Load Amount Statistics In Range',
    props<{ req: MessageAmountInRangeRequest }>()
);
export const getLoadAmountStatisticsInRangeSuccess = createAction(
    '[Statistics] Get Load Amount Statistics In Range Success',
    props<{ key: string, statistics: LoadAmountStatisticsResponse[], timespan: TimeSpan }>()
);
export const getLoadAmountStatisticsInRangeFailure = createAction(
    '[Statistics] Get Load Amount Statistics In Range Failure',
    props<{ error: any }>()
);

export const addLoadEventToLoadAmountStatistics = createAction(
    '[Statistics] Add Load Event To Load Amount Statistics',
    props<{ key: string, event: LoadEvent }>()
);

//#endregion

//#region Slot Statistics

export const downloadSlotStatistics = createAction(
    '[Statistics] Download Slot Statistics',
    props<{ key: string }>()
);
export const downLoadSlotStatisticsFailure = createAction(
    '[Statistics] Download Slot Statistics Failure',
    props<{ error: any }>()
);


//#endregion