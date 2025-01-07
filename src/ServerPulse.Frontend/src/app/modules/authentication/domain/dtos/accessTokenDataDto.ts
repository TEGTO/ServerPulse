import { AccessTokenData } from "../..";

export interface AccessTokenDataDto {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}

export function mapAuthTokenResponseToAuthToken(response: AccessTokenDataDto): AccessTokenData {
    return {
        accessToken: response?.accessToken,
        refreshToken: response?.refreshToken,
        refreshTokenExpiryDate: new Date(response?.refreshTokenExpiryDate)
    }
}