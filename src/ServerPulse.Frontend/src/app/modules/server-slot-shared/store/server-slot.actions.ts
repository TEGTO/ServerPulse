/* eslint-disable @typescript-eslint/no-explicit-any */
import { createAction, props } from "@ngrx/store";
import { CreateServerSlotRequest, ServerSlot, UpdateServerSlotRequest } from "..";

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

export const getUserServerSlots = createAction(
    '[Server Slot] Get User Server Slots',
    props<{ str?: string }>()
);
export const getUserServerSlotsSuccess = createAction(
    '[Server Slot] Get User Server Slots Success',
    props<{ serverSlots: ServerSlot[] }>()
);
export const getUserServerSlotsFailure = createAction(
    '[Server Slot] Get User Server Slots Failure',
    props<{ error: any }>()
);

export const createServerSlot = createAction(
    '[Server Slot] Create A New Server Slot',
    props<{ req: CreateServerSlotRequest }>()
);
export const createServerSlotSuccess = createAction(
    '[Server Slot] Create A New Server Slot Success',
    props<{ serverSlot: ServerSlot }>()
);
export const createServerSlotFailure = createAction(
    '[Server Slot] Create A New Server Slot Failure',
    props<{ error: any }>()
);

export const updateServerSlot = createAction(
    '[Server Slot] Update A Server Slot',
    props<{ req: UpdateServerSlotRequest }>()
);
export const updateServerSlotSuccess = createAction(
    '[Server Slot] Update A Server Slot Success',
    props<{ req: UpdateServerSlotRequest }>()
);
export const updateServerSlotFailure = createAction(
    '[Server Slot] Update A Server Slot Failure',
    props<{ error: any }>()
);

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

export const showSlotInfo = createAction(
    '[Server Slot] Show Server Slot Info By Id',
    props<{ id: string }>()
);

export const showSlotKey = createAction(
    '[Server Slot] Show Server Slot Key',
    props<{ slot: ServerSlot }>()
);