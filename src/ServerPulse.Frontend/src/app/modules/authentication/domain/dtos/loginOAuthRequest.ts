import { OAuthLoginProvider } from "../..";

export interface LoginOAuthRequest {
    queryParams: string;
    redirectUrl: string;
    oAuthLoginProvider: OAuthLoginProvider;
}