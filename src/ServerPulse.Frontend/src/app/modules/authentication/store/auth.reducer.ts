/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-explicit-any */
import { createReducer, on } from "@ngrx/store";
import { AuthData, copyAuthTokenToAuthData, copyUserUpdateRequestToUserAuth, getAuthDataFailure, getAuthDataSuccess, getDefaultAuthData, loginUser, loginUserFailure, loginUserSuccess, logOutUserSuccess, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerFailure, registerSuccess, registerUser, updateUserData, updateUserDataFailure, updateUserDataSuccess } from "..";

export interface AuthState {
    isRegistrationSuccessful: boolean,
    isUpdateSuccessful: boolean,
    isRefreshSuccessful: boolean,
    authData: AuthData,
    error: any
}
const initialAuthState: AuthState = {
    isRegistrationSuccessful: false,
    isUpdateSuccessful: false,
    isRefreshSuccessful: false,
    authData: getDefaultAuthData(),
    error: null
};

export const authReducer = createReducer(
    initialAuthState,

    on(registerUser, (state) => ({
        ...initialAuthState,
    })),
    on(registerSuccess, (state, { authData }) => ({
        ...state,
        authData: authData,
        isRegistrationSuccessful: true,
        error: null
    })),
    on(registerFailure, (state, { error }) => ({
        ...initialAuthState,
        error: error
    })),

    on(loginUser, () => ({
        ...initialAuthState
    })),
    on(loginUserSuccess, (state, { authData }) => ({
        ...state,
        authData: authData,
        error: null
    })),
    on(loginUserFailure, (state, { error }) => ({
        ...initialAuthState,
        error: error
    })),

    on(getAuthDataSuccess, (state, { authData }) => ({
        ...state,
        authData: authData,
        error: null
    })),
    on(getAuthDataFailure, (state) => ({
        ...initialAuthState
    })),

    on(logOutUserSuccess, (state) => ({
        ...initialAuthState,
    })),

    on(refreshAccessToken, (state) => ({
        ...state,
        isRefreshSuccessful: false,
        error: null
    })),
    on(refreshAccessTokenSuccess, (state, { authToken }) => ({
        ...state,
        isRefreshSuccessful: true,
        authData: copyAuthTokenToAuthData(state.authData, authToken),
        error: null
    })),
    on(refreshAccessTokenFailure, (state, { error }) => ({
        ...initialAuthState,
        error: error
    })),

    on(updateUserData, (state, { req: updateRequest }) => ({
        ...state,
        isUpdateSuccessful: false,
        error: null
    })),
    on(updateUserDataSuccess, (state, { req: updateRequest }) => ({
        ...state,
        isUpdateSuccessful: true,
        authData: copyUserUpdateRequestToUserAuth(state.authData, updateRequest),
        error: null
    })),
    on(updateUserDataFailure, (state, { error }) => ({
        ...state,
        isUpdateSuccessful: false,
        error: error
    })),
);