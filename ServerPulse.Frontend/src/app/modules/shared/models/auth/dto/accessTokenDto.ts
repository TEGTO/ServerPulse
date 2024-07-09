export interface AccessTokenDto {
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}