
export interface RefreshTokenRequest {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}