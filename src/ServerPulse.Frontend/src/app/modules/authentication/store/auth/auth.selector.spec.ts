import { AuthState, RegistrationState, UserDataState } from "./auth.reducer";
import { selectAuthData, selectAuthErrors, selectAuthState, selectIsRefreshSuccessful, selectIsRegistrationSuccess, selectIsUpdateSuccessful, selectRegistrationErrors, selectRegistrationState, selectUserDataState, selectUserErrors } from "./auth.selector";

describe('Registration Selectors', () => {
    const initialState: RegistrationState = {
        isSuccess: false,
        error: null
    };
    const errorState: RegistrationState = {
        isSuccess: false,
        error: 'An error occurred'
    };

    it('should select a registration state', () => {
        const result = selectRegistrationState.projector(initialState);
        expect(result).toEqual(initialState);
    });
    it('should select a registration is success', () => {
        const result = selectIsRegistrationSuccess.projector(initialState);
        expect(result).toEqual(initialState.isSuccess);
    });
    it('should select registration errors', () => {
        const result = selectRegistrationErrors.projector(errorState);
        expect(result).toEqual(errorState.error);
    });
});

describe('Authentication Selectors', () => {
    const initialState: AuthState = {
        isAuthenticated: false,
        accessToken: "",
        refreshToken: "",
        refreshTokenExpiryDate: new Date(),
        isRefreshSuccessful: false,
        error: null
    };
    const errorState: AuthState = {
        ...initialState,
        error: 'An error occurred'
    };

    it('should select an authentication state', () => {
        const result = selectAuthState.projector(initialState);
        expect(result).toEqual(initialState);
    });
    it('should select a authentication data', () => {
        const result = selectAuthData.projector(initialState);
        expect(result).toEqual({
            isAuthenticated: initialState.isAuthenticated,
            accessToken: initialState.accessToken,
            refreshToken: initialState.refreshToken,
            refreshTokenExpiryDate: initialState.refreshTokenExpiryDate
        });
    });
    it('should select selectIsRefreshSuccessful', () => {
        const result = selectIsRefreshSuccessful.projector(initialState);
        expect(result).toEqual(initialState.isRefreshSuccessful);
    });
    it('should select authentication errors', () => {
        const result = selectAuthErrors.projector(errorState);
        expect(result).toEqual(errorState.error);
    });
});

describe('User Data Selectors', () => {
    const initialState: UserDataState = {
        userName: "",
        email: "",
        isUpdateSuccess: false,
        error: null
    };
    const errorState: UserDataState = {
        ...initialState,
        error: 'An error occurred'
    };

    it('should select a user data state', () => {
        const result = selectUserDataState.projector(initialState);
        expect(result).toEqual(initialState);
    });
    it('should select is update successful data', () => {
        const result = selectIsUpdateSuccessful.projector(initialState);
        expect(result).toEqual(initialState.isUpdateSuccess);
    });
    it('should select user data errors', () => {
        const result = selectUserErrors.projector(errorState);
        expect(result).toEqual(errorState.error);
    });
});