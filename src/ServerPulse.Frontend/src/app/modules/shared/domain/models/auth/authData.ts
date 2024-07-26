export interface AuthData {
    isAuthenticated: boolean;
    accessToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}