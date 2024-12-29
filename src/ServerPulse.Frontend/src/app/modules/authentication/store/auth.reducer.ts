/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-explicit-any */
import { createReducer, on } from "@ngrx/store";
import { AuthData, authFailure, copyAuthTokenToAuthData, copyUserUpdateRequestToUserAuth, getAuthDataFailure, getAuthDataSuccess, getDefaultAuthData, loginUser, loginUserSuccess, logOutUserSuccess, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerUser, updateUserData, updateUserDataSuccess } from "..";

export interface AuthState {
    isRefreshSuccessful: boolean,
    authData: AuthData,
    error: any
}
const initialAuthState: AuthState = {
    isRefreshSuccessful: false,
    authData: getDefaultAuthData(),
    error: null
};

export const authReducer = createReducer(
    initialAuthState,

    on(authFailure, (state, { error }) => ({
        ...initialAuthState,
        error: error
    })),

    on(registerUser, (state) => ({
        ...initialAuthState,
    })),

    on(loginUser, () => ({
        ...initialAuthState
    })),
    on(loginUserSuccess, (state, { authData }) => ({
        ...state,
        authData: authData,
        error: null
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
        isRefreshSuccessful: false,
        error: error
    })),

    on(updateUserData, (state, { req: updateRequest }) => ({
        ...state,
        error: null
    })),
    on(updateUserDataSuccess, (state, { req: updateRequest }) => ({
        ...state,
        authData: copyUserUpdateRequestToUserAuth(state.authData, updateRequest),
        error: null
    })),
);