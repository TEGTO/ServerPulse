import { AuthData, LoginAccessTokenData, mapLoginAccessTokenDataToAuthToken } from "../..";

export interface LoginResponse {
    accessTokenData: LoginAccessTokenData;
    email: string;
}

export function mapLoginResponseToAuthData(response: LoginResponse): AuthData {
    return {
        isAuthenticated: true,
        accessTokenData: mapLoginAccessTokenDataToAuthToken(response?.accessTokenData),
        email: response?.email,
    }
}