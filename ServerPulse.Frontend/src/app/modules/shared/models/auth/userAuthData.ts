export interface UserAuthData {
    isAuthenticated: boolean;
    authToken: string;
    refreshToken: string;
    refreshTokenExpiryDate: Date;
    userEmail: string;
}