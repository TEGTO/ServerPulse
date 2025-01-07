import { AccessTokenDataDto, AuthData, mapAuthTokenResponseToAuthToken } from "../..";

export interface ConfirmEmailResponse {
    accessTokenData: AccessTokenDataDto;
    email: string;
}

export function mapConfirmEmailResponseToAuthData(response: ConfirmEmailResponse): AuthData {
    return {
        isAuthenticated: true,
        accessTokenData: mapAuthTokenResponseToAuthToken(response?.accessTokenData),
        email: response?.email,
    }
}