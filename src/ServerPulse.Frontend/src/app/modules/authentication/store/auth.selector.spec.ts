import { AuthData, AuthState, getDefaultAuthData, selectAuthData, selectAuthErrors, selectAuthState, selectIsRefreshSuccessful, selectIsRegistrationSuccessful, selectIsUpdateSuccessful } from "..";

describe('Authentication Selectors', () => {
    const initialState: AuthState = {
        isRegistrationSuccessful: false,
        isUpdateSuccessful: false,
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

    it('should select isRegistrationSuccessful', () => {
        const result = selectIsRegistrationSuccessful.projector(initialState);
        expect(result).toEqual(initialState.isRegistrationSuccessful);
    });

    it('should select isRefreshSuccessful', () => {
        const result = selectIsRefreshSuccessful.projector(initialState);
        expect(result).toEqual(initialState.isRefreshSuccessful);
    });

    it('should select isUpdateSuccessful', () => {
        const result = selectIsUpdateSuccessful.projector(initialState);
        expect(result).toEqual(initialState.isUpdateSuccessful);
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
        expect(result).toEqual(getDefaultAuthData());
    });

    it('should return updated authData when state has changes', () => {
        const updatedAuthData: AuthData = {
            isAuthenticated: true,
            authToken: {
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
