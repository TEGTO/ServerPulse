
export interface AuthToken {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}

export function getDefaultAuthToken(): AuthToken {
    return {
        accessToken: "",
        refreshToken: "",
        refreshTokenExpiryDate: new Date()
    }
}