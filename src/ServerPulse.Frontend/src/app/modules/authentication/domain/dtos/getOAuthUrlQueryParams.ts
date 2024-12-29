export enum OAuthLoginProvider {
    Google
}

export interface GetOAuthUrlQueryParams {
    oAuthLoginProvider: OAuthLoginProvider;
    redirectUrl: string;
    codeVerifier: string;
}