import { createAction, props } from "@ngrx/store";

//Slot Load Statistics 
export const selectDate = createAction(
    '[Slot Load Statistics] Select Date',
    props<{ date: Date }>()
);