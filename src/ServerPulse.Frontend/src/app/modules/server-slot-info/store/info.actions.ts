/* eslint-disable @typescript-eslint/no-explicit-any */
import { createAction, props } from "@ngrx/store";
import { CustomEvent, GetSomeMessagesRequest, LoadAmountStatistics, LoadEvent, MessageAmountInRangeRequest } from "../../analyzer";

export const setSelectedDate = createAction(
    '[Statistics] Set Selected Date ',
    props<{ date: Date, readFromDate: Date }>()
);

export const setReadFromDate = createAction(
    '[Statistics] Set Read From Date ',
    props<{ date: Date }>()
);

export const setCustomReadFromDate = createAction(
    '[Statistics] Set Custom Read From Date ',
    props<{ date: Date }>()
);

export const getSomeLoadEvents = createAction(
    '[Statistics] Get Some Load Events',
    props<{ req: GetSomeMessagesRequest }>()
);

export const getSomeLoadEventsSuccess = createAction(
    '[Statistics] Get Some Load Events Success',
    props<{ events: LoadEvent[] }>()
);

export const getSomeLoadEventsFailure = createAction(
    '[Statistics] Get Some Load Events Failure',
    props<{ error: any }>()
);

export const getSomeCustomEvents = createAction(
    '[Statistics] Get Some Custom Events',
    props<{ req: GetSomeMessagesRequest }>()
);

export const getSomeCustomEventsSuccess = createAction(
    '[Statistics] Get Some Custom Events Success',
    props<{ events: CustomEvent[] }>()
);

export const getSomeCustomEventsFailure = createAction(
    '[Statistics] Get Some Custom Events Failure',
    props<{ error: any }>()
);

export const getDailyLoadAmountStatistics = createAction(
    '[Statistics] Get Daily Load Amount Statistics',
    props<{ key: string }>()
);

export const getDailyLoadAmountStatisticsSuccess = createAction(
    '[Statistics] Get Daily Load Amount Statistics Success',
    props<{ statistics: LoadAmountStatistics[] }>()
);

export const getDailyLoadAmountStatisticsFailure = createAction(
    '[Statistics] Get Daily Load Amount Statistics Failure',
    props<{ error: any }>()
);

export const getLoadAmountStatisticsInRange = createAction(
    '[Statistics] Get Load Amount Statistics In Range',
    props<{ req: MessageAmountInRangeRequest }>()
);

export const getLoadAmountStatisticsInRangeSuccess = createAction(
    '[Statistics] Get Load Amount Statistics In Range Success',
    props<{ statistics: LoadAmountStatistics[] }>()
);

export const getLoadAmountStatisticsInRangeFailure = createAction(
    '[Statistics] Get Load Amount Statistics In Range Failure',
    props<{ error: any }>()
);

export const addNewLoadEvent = createAction(
    '[Statistics] Add New Load Event',
    props<{ event: LoadEvent }>()
);

export const addNewCustomEvent = createAction(
    '[Statistics] Add New Custom Event',
    props<{ event: CustomEvent }>()
);

export const showCustomDetailsEvent = createAction(
    '[Statistics] Show Custom Event Details',
    props<{ event: CustomEvent }>()
);