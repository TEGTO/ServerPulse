export interface AuthToken {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}