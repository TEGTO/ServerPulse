/* eslint-disable @typescript-eslint/no-explicit-any */
import { createAction, props } from "@ngrx/store";
import { AuthData, AuthToken, UserAuthenticationRequest, UserRegistrationRequest, UserUpdateRequest } from "..";

export const startRegisterUser = createAction(
    '[Auth] Start Register New User'
);

export const registerUser = createAction(
    '[Auth] Register New User',
    props<{ req: UserRegistrationRequest }>()
);
export const registerSuccess = createAction(
    '[Auth] Register New User Success',
    props<{ authData: AuthData }>()
);
export const registerFailure = createAction(
    '[Auth] Register New User Failure',
    props<{ error: any }>()
);

export const startLoginUser = createAction(
    '[Auth] Start Login User'
);

export const loginUser = createAction(
    '[Auth] Login By User',
    props<{ req: UserAuthenticationRequest }>()
);
export const loginUserSuccess = createAction(
    '[Auth] Login By User Success',
    props<{ authData: AuthData }>()
);
export const loginUserFailure = createAction(
    '[Auth] Login By User Failure',
    props<{ error: any }>()
);

export const getAuthData = createAction(
    '[Auth] Get Authenticated Data'
);
export const getAuthDataSuccess = createAction(
    '[Auth] Get Authenticated Data Success',
    props<{ authData: AuthData }>()
);
export const getAuthDataFailure = createAction(
    '[Auth] Get Authenticated Data Failure'
);

export const logOutUser = createAction(
    '[Auth] Log out Authenticated User'
);
export const logOutUserSuccess = createAction(
    '[Auth] Log out Authenticated User Success'
);

export const refreshAccessToken = createAction(
    '[Auth] Refresh Access Token',
    props<{ authToken: AuthToken }>()
);
export const refreshAccessTokenSuccess = createAction(
    '[Auth] Refresh Access Token Success',
    props<{ authToken: AuthToken }>()
);
export const refreshAccessTokenFailure = createAction(
    '[Auth] Refresh Access Token Failure',
    props<{ error: any }>()
);

export const updateUserData = createAction(
    '[Auth] Update User Data',
    props<{ req: UserUpdateRequest }>()
);
export const updateUserDataSuccess = createAction(
    '[Auth] Update User Data Success',
    props<{ req: UserUpdateRequest }>()
);
export const updateUserDataFailure = createAction(
    '[Auth] Update User Data Failure',
    props<{ error: any }>()
);

