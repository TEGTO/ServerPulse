/* eslint-disable @typescript-eslint/no-explicit-any */
import { createAction, props } from "@ngrx/store";
import { AuthData, AuthToken, EmailConfirmationRequest, OAuthLoginProvider, UserAuthenticationRequest, UserRegistrationRequest, UserUpdateRequest } from "..";

export const authFailure = createAction(
    '[Auth] Auth Operation Failure',
    props<{ error: any }>()
);

export const startRegisterUser = createAction(
    '[Auth] Start Register New User'
);

export const registerUser = createAction(
    '[Auth] Register New User',
    props<{ req: UserRegistrationRequest }>()
);
export const registerUserSuccess = createAction(
    '[Auth] Register New User Success'
);

export const confirmEmail = createAction(
    '[Auth] Cofirm User Email',
    props<{ req: EmailConfirmationRequest }>()
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

export const startOAuthLogin = createAction(
    '[OAuth] Start OAuth Login',
    props<{ loginProvider: OAuthLoginProvider }>()
);
export const startOAuthLoginFailure = createAction(
    '[OAuth] Start OAuth Login Failure',
    props<{ error: any }>()
);

export const oauthLogin = createAction(
    '[OAuth] OAuth Login',
    props<{ code: string }>()
);
export const oauthLoginFailure = createAction(
    '[OAuth] OAuth Login Failure',
    props<{ error: any }>()
);


