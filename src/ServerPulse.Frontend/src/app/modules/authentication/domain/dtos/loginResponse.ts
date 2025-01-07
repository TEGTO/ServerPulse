import { AccessTokenDataDto, AuthData, mapAuthTokenResponseToAuthToken } from "../..";

export interface LoginResponse {
    accessTokenData: AccessTokenDataDto;
    email: string;
}

export function mapLoginResponseToAuthData(response: LoginResponse): AuthData {
    return {
        isAuthenticated: true,
        accessTokenData: mapAuthTokenResponseToAuthToken(response?.accessTokenData),
        email: response?.email,
    }
}