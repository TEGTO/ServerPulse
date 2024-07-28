import { AuthToken } from "../../dtos/auth/authToken";

export interface AuthData {
    isAuthenticated: boolean;
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}

export function getAuthDataFromAuthToken(authToken: AuthToken): AuthData {
    return {
        isAuthenticated: true,
        accessToken: authToken.accessToken,
        refreshToken: authToken.refreshToken,
        refreshTokenExpiryDate: authToken.refreshTokenExpiryDate
    };
}