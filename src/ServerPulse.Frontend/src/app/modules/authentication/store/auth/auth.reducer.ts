import { createReducer, on } from "@ngrx/store";
import { getAuthDataFailure, getAuthDataSuccess, logOutUserSuccess, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerFailure, registerSuccess, registerUser, signInUser, signInUserFailure, signInUserSuccess, updateUserData, updateUserDataFailure, updateUserDataSuccess } from "../..";

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
        ...initialRegistrationState,
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
    accessToken: string,
    refreshToken: string,
    refreshTokenExpiryDate: Date,
    error: any
}
const initialAuthState: AuthState = {
    isAuthenticated: false,
    accessToken: "",
    refreshToken: "",
    refreshTokenExpiryDate: new Date(),
    error: null
};

export const authReducer = createReducer(
    initialAuthState,
    on(signInUser, (state) => ({
        ...initialAuthState
    })),
    on(signInUserSuccess, (state, { authData: authData }) => ({
        ...state,
        isAuthenticated: true,
        accessToken: authData.accessToken,
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
        accessToken: authData.accessToken,
        refreshToken: authData.refreshToken,
        refreshTokenExpiryDate: authData.refreshTokenExpiryDate,
        error: null
    })),
    on(getAuthDataFailure, (state) => ({
        ...initialAuthState
    })),

    on(logOutUserSuccess, (state) => ({
        ...initialAuthState,
    })),

    on(refreshAccessTokenSuccess, (state, { authToken: accessToken }) => ({
        ...state,
        isAuthenticated: true,
        accessToken: accessToken.accessToken,
        refreshToken: accessToken.refreshToken,
        refreshTokenExpiryDate: accessToken.refreshTokenExpiryDate,
        error: null
    })),
    on(refreshAccessTokenFailure, (state, { error: error }) => ({
        ...initialAuthState,
        error: error
    })),
);
//UserData
export interface UserDataState {
    userName: string,
    email: string,
    isUpdateSuccess: boolean,
    error: any
}
const initialUserState: UserDataState = {
    userName: "",
    email: "",
    isUpdateSuccess: false,
    error: null
};

export const userDataReducer = createReducer(
    initialUserState,
    on(signInUser, (state) => ({
        ...initialUserState
    })),
    on(signInUserSuccess, (state, { userData: userData }) => ({
        ...state,
        userName: userData.userName,
        email: userData.email,
        error: null
    })),
    on(signInUserFailure, (state) => ({
        ...initialUserState
    })),

    on(getAuthDataSuccess, (state, { userData: userData }) => ({
        ...state,
        userName: userData.userName,
        email: userData.email,
        error: null
    })),
    on(getAuthDataFailure, (state) => ({
        ...initialUserState
    })),

    on(logOutUserSuccess, (state) => ({
        ...initialUserState,
    })),

    on(updateUserData, (state) => ({
        ...state,
        isUpdateSuccess: false,
        error: null
    })),
    on(updateUserDataSuccess, (state, { userData: userData }) => ({
        ...state,
        userName: userData.userName,
        email: userData.email,
        isUpdateSuccess: true,
        error: null
    })),
    on(updateUserDataFailure, (state, { error: error }) => ({
        ...state,
        isUpdateSuccess: false,
        error: error
    })),
    on(refreshAccessTokenFailure, (state) => ({
        ...initialUserState
    })),
);