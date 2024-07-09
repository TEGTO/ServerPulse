import { createReducer, on } from "@ngrx/store";
import { getAuthUserDataSuccess, logOutUserSuccess, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerFailure, registerSuccess, signInUser, signInUserFailure, signInUserSuccess, updateUserDataFailure, updateUserDataSuccess } from "../..";

//Registration
export interface RegistrationState {
    isSuccess: boolean,
    error: any
}
const initialRegistrationState: RegistrationState = {
    isSuccess: true,
    error: null
};
export const registrationReducer = createReducer(
    initialRegistrationState,
    on(registerSuccess, (state) => ({
        ...state,
        isSuccess: true,
        error: null
    })),
    on(registerFailure, (state, { error: error }) => ({
        ...state,
        isSuccess: false,
        error: error
    })),
);
//Auth
export interface AuthState {
    isAuthenticated: boolean,
    authToken: string,
    refreshToken: string,
    refreshTokenExpiryDate: Date,
    userEmail: string,
    error: any
}
const initialAuthState: AuthState = {
    isAuthenticated: false,
    authToken: "",
    refreshToken: "",
    userEmail: "",
    refreshTokenExpiryDate: new Date(),
    error: null
};

export const authReducer = createReducer(
    initialAuthState,
    on(signInUser, (state) => ({
        ...initialAuthState,
        isAuthenticated: false
    })),
    on(signInUserSuccess, (state, { userAuthData: userAuthData }) => ({
        ...state,
        isAuthenticated: true,
        authToken: userAuthData.authToken,
        refreshToken: userAuthData.refreshToken,
        refreshTokenExpiryDate: userAuthData.refreshTokenExpiryDate,
        userEmail: userAuthData.userEmail,
        error: null
    })),
    on(signInUserFailure, (state, { error: error }) => ({
        ...initialAuthState,
        error: error
    })),

    on(getAuthUserDataSuccess, (state, { userAuthData: userAuthData }) => ({
        ...state,
        isAuthenticated: userAuthData.isAuthenticated,
        authToken: userAuthData.authToken,
        refreshToken: userAuthData.refreshToken,
        refreshTokenExpiryDate: userAuthData.refreshTokenExpiryDate,
        userEmail: userAuthData.userEmail,
        error: null
    })),

    on(logOutUserSuccess, (state) => ({
        ...initialAuthState,
    })),

    on(refreshAccessTokenSuccess, (state, { accessToken: accessToken }) => ({
        ...state,
        isAuthenticated: true,
        authToken: accessToken.accessToken,
        refreshToken: accessToken.refreshToken,
        refreshTokenExpiryDate: accessToken.refreshTokenExpiryDate,
        error: null
    })),
    on(refreshAccessTokenFailure, (state, { error: error }) => ({
        ...initialAuthState,
        error: error
    })),

    on(updateUserDataSuccess, (state, { userUpdateData: userUpdateData }) => ({
        ...state,
        userEmail: userUpdateData.newEmail ? userUpdateData.newEmail : userUpdateData.oldEmail,
        error: null
    })),
    on(updateUserDataFailure, (state, { error: error }) => ({
        ...state,
        error: error
    })),
);