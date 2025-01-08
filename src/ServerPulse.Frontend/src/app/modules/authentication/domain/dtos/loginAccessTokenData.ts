import { AccessTokenData } from "../..";

export interface LoginAccessTokenData {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}

export function mapLoginAccessTokenDataToAuthToken(response: LoginAccessTokenData): AccessTokenData {
    return {
        accessToken: response?.accessToken,
        refreshToken: response?.refreshToken,
        refreshTokenExpiryDate: new Date(response?.refreshTokenExpiryDate)
    }
}