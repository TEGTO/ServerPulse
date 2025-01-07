import { AuthData, AuthState, getDefaultAuthData, selectAuthData, selectAuthErrors, selectAuthState, selectIsRefreshSuccessful } from "..";

describe('Authentication Selectors', () => {
    const initialState: AuthState = {
        isRefreshSuccessful: false,
        authData: getDefaultAuthData(),
        error: null,
    };

    const errorState: AuthState = {
        ...initialState,
        error: 'An error occurred',
    };

    it('should select the authentication state', () => {
        const result = selectAuthState.projector(initialState);
        expect(result).toEqual(initialState);
    });

    it('should select authData', () => {
        const result = selectAuthData.projector(initialState);
        expect(result).toEqual(initialState.authData);
    });

    it('should select isRefreshSuccessful', () => {
        const result = selectIsRefreshSuccessful.projector(initialState);
        expect(result).toEqual(initialState.isRefreshSuccessful);
    });

    it('should select auth errors', () => {
        const result = selectAuthErrors.projector(errorState);
        expect(result).toEqual(errorState.error);
    });

    it('should return default authData when state is empty', () => {
        const emptyState: AuthState = {
            ...initialState,
            authData: getDefaultAuthData(),
        };
        const result = selectAuthData.projector(emptyState);
        expect(result.accessTokenData.accessToken).toEqual(getDefaultAuthData().accessTokenData.accessToken);
    });

    it('should return updated authData when state has changes', () => {
        const updatedAuthData: AuthData = {
            isAuthenticated: true,
            accessTokenData: {
                accessToken: 'newAccessToken',
                refreshToken: 'newRefreshToken',
                refreshTokenExpiryDate: new Date(),
            },
            email: 'updated@example.com',
        };
        const updatedState: AuthState = {
            ...initialState,
            authData: updatedAuthData,
        };

        const result = selectAuthData.projector(updatedState);
        expect(result).toEqual(updatedAuthData);
    });
});
