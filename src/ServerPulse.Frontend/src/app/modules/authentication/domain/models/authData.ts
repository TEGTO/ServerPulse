import { AuthToken, getDefaultAuthToken } from "../..";

export interface AuthData {
    isAuthenticated: boolean,
    authToken: AuthToken;
    email: string;
}

export function copyAuthTokenToAuthData(authData: AuthData, authToken: AuthToken): AuthData {
    return {
        ...authData,
        authToken: authToken
    };
}
export function getDefaultAuthData(): AuthData {
    return {
        isAuthenticated: false,
        authToken: getDefaultAuthToken(),
        email: "",
    }
}