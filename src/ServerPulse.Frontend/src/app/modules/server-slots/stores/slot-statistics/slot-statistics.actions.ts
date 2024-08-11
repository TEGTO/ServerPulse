import { createAction, props } from "@ngrx/store";

//Slot Statistics
export const subscribeToSlotStatistics = createAction(
    '[Slot Statistics] Subscribe To Slot Statistics',
    props<{ slotKey: string }>()
);
export const subscribeToSlotStatisticsSuccess = createAction(
    '[Slot Statistics] Subscribe To Slot Statistics Success',
    props<{ lastStatistics: { key: string; data: string; } }>()
);
export const subscribeToSlotStatisticsFailure = createAction(
    '[Slot Statistics] Subscribe To Slot Statistics Failure',
    props<{ error: any }>()
);

//Slot Load Statistics 
export const selectDate = createAction(
    '[Slot Load Statistics] Select Date',
    props<{ date: Date }>()
);

export const subscribeToLoadStatistics = createAction(
    '[Slot Load Statistics] Subscribe To Load Statistics',
    props<{ slotKey: string }>()
);
export const subscribeToLoadStatisticsSuccess = createAction(
    '[Slot Load Statistics] Subscribe To Load Statistics Success',
    props<{ lastLoadStatistics: { key: string; data: string; } }>()
);
export const subscribeToLoadStatisticsFailure = createAction(
    '[Slot Load Statistics] Subscribe To Load Statistics Failure',
    props<{ error: any }>()
);