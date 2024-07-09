import { createAction, props } from "@ngrx/store";
import { AccessTokenDto, UserAuthData, UserAuthenticationRequest, UserRegistrationRequest, UserUpdateDataRequest } from "../../../shared";

//Registration
export const registerUser = createAction(
    '[Registration] Register New User',
    props<{ userRegistrationData: UserRegistrationRequest }>()
);
export const registerSuccess = createAction(
    '[Registration] Register New User Success'
);
export const registerFailure = createAction(
    '[Registration] Register New User Failure',
    props<{ error: any }>()
);
//Auth
export const signInUser = createAction(
    '[Auth] Sing In By User Data',
    props<{ userAuthData: UserAuthenticationRequest }>()
);
export const signInUserSuccess = createAction(
    '[Auth] Sing In By User Data Success',
    props<{ userAuthData: UserAuthData }>()
);
export const signInUserFailure = createAction(
    '[Auth] Sing In By User Data Failure',
    props<{ error: any }>()
);

export const getAuthUserData = createAction(
    '[Auth] Get Authenticated User Data'
);
export const getAuthUserDataSuccess = createAction(
    '[Auth] Get Authenticated User Data Success',
    props<{ userAuthData: UserAuthData }>()
);

export const logOutUser = createAction(
    '[Auth] Log out Authenticated User'
);
export const logOutUserSuccess = createAction(
    '[Auth] Log out Authenticated User Success'
);

export const refreshAccessToken = createAction(
    '[Auth] Refresh Access Token',
    props<{ accessToken: AccessTokenDto }>()
);
export const refreshAccessTokenSuccess = createAction(
    '[Auth] Refresh Access Token Success',
    props<{ accessToken: AccessTokenDto }>()
);
export const refreshAccessTokenFailure = createAction(
    '[Auth] Refresh Access Token  Failure',
    props<{ error: any }>()
);

export const updateUserData = createAction(
    '[Auth] Update Authenticated User',
    props<{ userUpdateData: UserUpdateDataRequest }>()
);
export const updateUserDataSuccess = createAction(
    '[Auth] Update Authenticated User Success',
    props<{ userUpdateData: UserUpdateDataRequest }>()
);
export const updateUserDataFailure = createAction(
    '[Auth] Update Authenticated User Failure',
    props<{ error: any }>()
);