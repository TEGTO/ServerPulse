import { AuthData, AuthTokenResponse, mapAuthTokenResponseToAuthToken } from "../..";

export interface UserAuthenticationResponse {
    authToken: AuthTokenResponse;
    email: string;
}

export function mapUserAuthenticationResponseToUserAuthentication(response: UserAuthenticationResponse): AuthData {
    return {
        isAuthenticated: true,
        authToken: mapAuthTokenResponseToAuthToken(response?.authToken),
        email: response?.email,
    }
}