export interface AuthData {
    isAuthenticated: boolean;
    authToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
}