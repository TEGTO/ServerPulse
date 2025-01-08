import { OAuthLoginProvider } from "../..";

export interface LoginOAuthRequest {
    code: string;
    codeVerifier: string;
    redirectUrl: string;
    oAuthLoginProvider: OAuthLoginProvider;
}