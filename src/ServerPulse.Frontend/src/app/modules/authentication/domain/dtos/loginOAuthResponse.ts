import { AccessTokenDataDto, AuthData, mapAuthTokenResponseToAuthToken } from "../..";

export interface LoginOAuthResponse {
    accessTokenData: AccessTokenDataDto;
    email: string;
}

export function mapUserAuthenticationResponseToAuthData(response: LoginOAuthResponse): AuthData {
    return {
        isAuthenticated: true,
        accessTokenData: mapAuthTokenResponseToAuthToken(response?.accessTokenData),
        email: response?.email,
    }
}