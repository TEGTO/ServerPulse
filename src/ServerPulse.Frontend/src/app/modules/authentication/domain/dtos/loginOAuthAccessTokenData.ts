import { AccessTokenData } from "../..";

export interface LoginOAuthAccessTokenData {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}

export function mapLoginOAuthAccessTokenDataToAuthToken(response: LoginOAuthAccessTokenData): AccessTokenData {
    return {
        accessToken: response?.accessToken,
        refreshToken: response?.refreshToken,
        refreshTokenExpiryDate: new Date(response?.refreshTokenExpiryDate)
    }
}