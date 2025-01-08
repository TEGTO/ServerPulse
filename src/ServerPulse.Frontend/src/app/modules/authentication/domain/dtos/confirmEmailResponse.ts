import { AuthData, ConfirmEmailAccessTokenData, mapConfirmEmailAccessTokenDataToAuthToken } from "../..";

export interface ConfirmEmailResponse {
    accessTokenData: ConfirmEmailAccessTokenData;
    email: string;
}

export function mapConfirmEmailResponseToAuthData(response: ConfirmEmailResponse): AuthData {
    return {
        isAuthenticated: true,
        accessTokenData: mapConfirmEmailAccessTokenDataToAuthToken(response?.accessTokenData),
        email: response?.email,
    }
}