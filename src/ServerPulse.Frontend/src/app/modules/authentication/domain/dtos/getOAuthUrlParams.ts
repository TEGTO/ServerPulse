export enum OAuthLoginProvider {
    Google,
    GitHub
}

export interface GetOAuthUrlParams {
    oAuthLoginProvider: OAuthLoginProvider;
    redirectUrl: string;
}