export enum OAuthLoginProvider {
    Google
}

export interface GetOAuthUrlParams {
    oAuthLoginProvider: OAuthLoginProvider;
    redirectUrl: string;
}