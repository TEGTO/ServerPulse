import { AuthData, LoginOAuthAccessTokenData, mapLoginOAuthAccessTokenDataToAuthToken } from "../..";

export interface LoginOAuthResponse {
    accessTokenData: LoginOAuthAccessTokenData;
    email: string;
}

export function mapUserAuthenticationResponseToAuthData(response: LoginOAuthResponse): AuthData {
    return {
        isAuthenticated: true,
        accessTokenData: mapLoginOAuthAccessTokenDataToAuthToken(response?.accessTokenData),
        email: response?.email,
    }
}