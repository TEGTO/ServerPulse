import { AccessTokenData, getDefaultAccessTokenData } from "../..";

export interface AuthData {
    isAuthenticated: boolean,
    accessTokenData: AccessTokenData;
    email: string;
}

export function copyAccessTokenDataToAuthData(authData: AuthData, accessTokenData: AccessTokenData): AuthData {
    return {
        ...authData,
        accessTokenData: accessTokenData
    };
}
export function getDefaultAuthData(): AuthData {
    return {
        isAuthenticated: false,
        accessTokenData: getDefaultAccessTokenData(),
        email: "",
    }
}