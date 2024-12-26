import { AuthToken } from "../..";

export interface AuthTokenResponse {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}

export function mapAuthTokenResponseToAuthToken(response: AuthTokenResponse): AuthToken {
    return {
        accessToken: response?.accessToken,
        refreshToken: response?.refreshToken,
        refreshTokenExpiryDate: new Date(response?.refreshTokenExpiryDate)
    }
}