
export interface AccessTokenData {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}

export function getDefaultAccessTokenData(): AccessTokenData {
    return {
        accessToken: "",
        refreshToken: "",
        refreshTokenExpiryDate: new Date()
    }
}