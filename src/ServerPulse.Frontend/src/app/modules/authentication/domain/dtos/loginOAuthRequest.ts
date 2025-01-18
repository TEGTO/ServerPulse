import { OAuthLoginProvider } from "../..";

export interface LoginOAuthRequest {
    code: string;
    redirectUrl: string;
    oAuthLoginProvider: OAuthLoginProvider;
}