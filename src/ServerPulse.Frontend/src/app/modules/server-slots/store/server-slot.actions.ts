import { createAction, props } from "@ngrx/store";
import { CreateServerSlotRequest } from "../../shared/domain/dtos/server-slot/createServerSlotRequest";
import { UpdateServerSlotRequest } from "../../shared/domain/dtos/server-slot/updateServerSlotRequest";
import { ServerSlot } from "../../shared/domain/models/server-slot/serverSlot";

//Get Server Slot By Id
export const getServerSlotById = createAction(
    '[Server Slot] Get Server Slot By Id',
    props<{ id: string }>()
);
export const getServerSlotByIdSuccess = createAction(
    '[Server Slot] Get Server Slot By Id Success',
    props<{ serverSlot: ServerSlot }>()
);
export const getServerSlotByIdFailure = createAction(
    '[Server Slot] Get Server Slot By Id Failure',
    props<{ error: any }>()
);
//Get Server Slots
export const getServerSlots = createAction(
    '[Server Slot] Get Server Slots Of The User',
);
export const getServerSlotsSuccess = createAction(
    '[Server Slot] Get Server Slots Of The User Success',
    props<{ serverSlots: ServerSlot[] }>()
);
export const getServerSlotsFailure = createAction(
    '[Server Slot] Get Server Slots Of The User Failure',
    props<{ error: any }>()
);
//Get Server Slots With String 
export const getServerSlotsWithString = createAction(
    '[Server Slot] Get Server Slots That Contain The String',
    props<{ str: string }>()
);
export const getServerSlotsWithStringSuccess = createAction(
    '[Server Slot] Get Server Slots That Contain The String Success',
    props<{ serverSlots: ServerSlot[] }>()
);
export const getServerSlotsWithStringFailure = createAction(
    '[Server Slot] Get Server Slots That Contain The String Failure',
    props<{ error: any }>()
);
//Create A New Server Slot
export const createServerSlot = createAction(
    '[Server Slot] Create A New Server Slot',
    props<{ createRequest: CreateServerSlotRequest }>()
);
export const createServerSlotSuccess = createAction(
    '[Server Slot] Create A New Server Slot Success',
    props<{ serverSlot: ServerSlot }>()
);
export const createServerSlotFailure = createAction(
    '[Server Slot] Create A New Server Slot Failure',
    props<{ error: any }>()
);
//Update A Server Slot
export const updateServerSlot = createAction(
    '[Server Slot] Update A Server Slot',
    props<{ updateRequest: UpdateServerSlotRequest }>()
);
export const updateServerSlotSuccess = createAction(
    '[Server Slot] Update A Server Slot Success',
    props<{ updateRequest: UpdateServerSlotRequest }>()
);
export const updateServerSlotFailure = createAction(
    '[Server Slot] Update A Server Slot Failure',
    props<{ error: any }>()
);
//Delete A Server Slot
export const deleteServerSlot = createAction(
    '[Server Slot] Delete A Server Slot',
    props<{ id: string }>()
);
export const deleteServerSlotSuccess = createAction(
    '[Server Slot] Delete A Server Slot Success',
    props<{ id: string }>()
);
export const deleteServerSlotFailure = createAction(
    '[Server Slot] Delete A Server Slot Failure',
    props<{ error: any }>()
);
