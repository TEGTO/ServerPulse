import { AccessTokenData } from "../..";

export interface ConfirmEmailAccessTokenData {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}

export function mapConfirmEmailAccessTokenDataToAuthToken(response: ConfirmEmailAccessTokenData): AccessTokenData {
    return {
        accessToken: response?.accessToken,
        refreshToken: response?.refreshToken,
        refreshTokenExpiryDate: new Date(response?.refreshTokenExpiryDate)
    }
}