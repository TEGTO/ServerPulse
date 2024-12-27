import { OAuthLoginProvider } from "../..";

export interface UserOAuthenticationRequest {
    code: string;
    codeVerifier: string;
    redirectUrl: string;
    oAuthLoginProvider: OAuthLoginProvider;
}