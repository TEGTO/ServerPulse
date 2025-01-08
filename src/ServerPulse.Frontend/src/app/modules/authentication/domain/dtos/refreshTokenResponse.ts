import { AccessTokenData } from "../..";

export interface RefreshTokenResponse {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}

export function mapRefreshTokenResponseToAuthToken(response: RefreshTokenResponse): AccessTokenData {
    return {
        accessToken: response?.accessToken,
        refreshToken: response?.refreshToken,
        refreshTokenExpiryDate: new Date(response?.refreshTokenExpiryDate)
    }
}