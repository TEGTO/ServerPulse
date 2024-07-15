import { createReducer, on } from "@ngrx/store";
import { getAuthDataSuccess, logOutUserSuccess, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerFailure, registerSuccess, registerUser, signInUser, signInUserFailure, signInUserSuccess, updateUserDataFailure, updateUserDataSuccess } from "../..";

//Registration
export interface RegistrationState {
    isSuccess: boolean,
    error: any
}
const initialRegistrationState: RegistrationState = {
    isSuccess: false,
    error: null
};
export const registrationReducer = createReducer(
    initialRegistrationState,
    on(registerUser, (state) => ({
        ...state,
        initialRegistrationState
    })),
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
    error: any
}
const initialAuthState: AuthState = {
    isAuthenticated: false,
    authToken: "",
    refreshToken: "",
    refreshTokenExpiryDate: new Date(),
    error: null
};

export const authReducer = createReducer(
    initialAuthState,
    on(signInUser, (state) => ({
        ...initialAuthState,
        isAuthenticated: false
    })),
    on(signInUserSuccess, (state, { authData: authData }) => ({
        ...state,
        isAuthenticated: true,
        authToken: authData.authToken,
        refreshToken: authData.refreshToken,
        refreshTokenExpiryDate: authData.refreshTokenExpiryDate,
        error: null
    })),
    on(signInUserFailure, (state, { error: error }) => ({
        ...initialAuthState,
        error: error
    })),

    on(getAuthDataSuccess, (state, { authData: authData }) => ({
        ...state,
        isAuthenticated: authData.isAuthenticated,
        authToken: authData.authToken,
        refreshToken: authData.refreshToken,
        refreshTokenExpiryDate: authData.refreshTokenExpiryDate,
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

    on(updateUserDataSuccess, (state, { userData: userData }) => ({
        ...state,
        userEmail: userData.email,
        error: null
    })),
    on(updateUserDataFailure, (state, { error: error }) => ({
        ...state,
        error: error
    })),
);
//UserData
export interface UserDataState {
    userName: string,
    email: string,
    error: any
}
const initialUserState: UserDataState = {
    userName: "",
    email: "",
    error: null
};

export const userDataReducer = createReducer(
    initialUserState,
    on(signInUserSuccess, (state, { userData: userData }) => ({
        ...state,
        userName: userData.userName,
        email: userData.email,
        error: null
    })),
    on(signInUserFailure, (state, { error: error }) => ({
        ...initialUserState,
        error: error
    })),

    on(getAuthDataSuccess, (state, { userData: userData }) => ({
        ...state,
        userName: userData.userName,
        email: userData.email,
        error: null
    })),

    on(logOutUserSuccess, (state) => ({
        ...initialUserState,
    })),

    on(updateUserDataSuccess, (state, { userData: userData }) => ({
        ...state,
        userName: userData.userName,
        email: userData.email,
        error: null
    })),
    on(updateUserDataFailure, (state, { error: error }) => ({
        ...state,
        error: error
    })),
);