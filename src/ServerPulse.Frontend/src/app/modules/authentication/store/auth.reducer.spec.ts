/* eslint-disable @typescript-eslint/no-explicit-any */
import { authReducer, AuthState, copyUserUpdateRequestToUserAuth, getAuthData, getAuthDataFailure, getAuthDataSuccess, getDefaultAuthData, getDefaultAuthToken, loginUser, loginUserFailure, loginUserSuccess, logOutUserSuccess, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerFailure, registerSuccess, registerUser, startLoginUser, startRegisterUser, updateUserData, updateUserDataFailure, updateUserDataSuccess } from "..";

describe('Auth Reducer', () => {
    const initialState: AuthState = {
        isRegistrationSuccessful: false,
        isUpdateSuccessful: false,
        isRefreshSuccessful: false,
        authData: getDefaultAuthData(),
        error: null,
    };

    it('should return the initial state by default', () => {
        const action = { type: 'UNKNOWN' } as any;
        const state = authReducer(initialState, action);

        expect(state).toEqual(initialState);
    });

    describe('Registration Actions', () => {
        it('should reset state on startRegisterUser', () => {
            const action = startRegisterUser();
            const state = authReducer(initialState, action);

            expect(state).toEqual(initialState);
        });

        it('should reset state on registerUser', () => {
            const action = registerUser({ req: { email: 'test@example.com', password: 'password', confirmPassword: 'password' } });
            const state = authReducer(initialState, action);

            expect(state.isRegistrationSuccessful).toEqual(false);
            expect(state.authData.email).toEqual("");
        });

        it('should set registration success on registerSuccess', () => {
            const authData = { isAuthenticated: true, authToken: getDefaultAuthToken(), email: 'test@example.com' };
            const action = registerSuccess({ authData });
            const state = authReducer(initialState, action);

            expect(state.isRegistrationSuccessful).toBe(true);
            expect(state.authData).toEqual(authData);
            expect(state.error).toBeNull();
        });

        it('should set error on registerFailure', () => {
            const error = 'Registration failed';
            const action = registerFailure({ error });
            const state = authReducer(initialState, action);

            expect(state.error).toEqual(error);
        });
    });

    describe('Login Actions', () => {
        it('should reset state on startLoginUser', () => {
            const action = startLoginUser();
            const state = authReducer(initialState, action);

            expect(state).toEqual(initialState);
        });

        it('should reset state on loginUser', () => {
            const action = loginUser({ req: { login: 'user', password: 'password' } });
            const state = authReducer(initialState, action);

            expect(state.isRegistrationSuccessful).toEqual(false);
            expect(state.authData.email).toEqual("");
        });

        it('should update authData on loginUserSuccess', () => {
            const authData = { isAuthenticated: true, authToken: getDefaultAuthToken(), email: 'test@example.com' };
            const action = loginUserSuccess({ authData });
            const state = authReducer(initialState, action);

            expect(state.authData).toEqual(authData);
            expect(state.error).toBeNull();
        });

        it('should set error on loginUserFailure', () => {
            const error = 'Login failed';
            const action = loginUserFailure({ error: error });
            const state = authReducer(initialState, action);

            expect(state.error).toEqual(error);
        });
    });

    describe('Auth Data Actions', () => {
        it('should reset state on getAuthData', () => {
            const action = getAuthData();
            const state = authReducer(initialState, action);

            expect(state).toEqual(initialState);
        });

        it('should update authData on getAuthDataSuccess', () => {
            const authData = { isAuthenticated: true, authToken: getDefaultAuthToken(), email: 'test@example.com' };
            const action = getAuthDataSuccess({ authData });
            const state = authReducer(initialState, action);

            expect(state.authData).toEqual(authData);
            expect(state.error).toBeNull();
        });

        it('should reset state on getAuthDataFailure', () => {
            const action = getAuthDataFailure();
            const state = authReducer(initialState, action);

            expect(state.isRegistrationSuccessful).toEqual(false);
            expect(state.authData.email).toEqual("");
        });
    });

    describe('Logout Actions', () => {
        it('should reset state on logOutUserSuccess', () => {
            const action = logOutUserSuccess();
            const state = authReducer(initialState, action);

            expect(state.isRegistrationSuccessful).toEqual(false);
            expect(state.authData.email).toEqual("");
        });
    });

    describe('Refresh Token Actions', () => {
        it('should set refresh flag on refreshAccessToken', () => {
            const authToken = { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() };
            const action = refreshAccessToken({ authToken });
            const state = authReducer(initialState, action);

            expect(state.isRefreshSuccessful).toBe(false);
            expect(state.error).toBeNull();
        });

        it('should update authData and set refresh flag on refreshAccessTokenSuccess', () => {
            const authToken = { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() };
            const action = refreshAccessTokenSuccess({ authToken });
            const state = authReducer(initialState, action);

            expect(state.isRefreshSuccessful).toBe(true);
            expect(state.authData.authToken).toEqual(authToken);
            expect(state.error).toBeNull();
        });

        it('should reset state and set error on refreshAccessTokenFailure', () => {
            const error = 'Refresh token failed';
            const action = refreshAccessTokenFailure({ error });
            const state = authReducer(initialState, action);

            expect(state.error).toEqual(error);
        });
    });

    describe('Update User Data Actions', () => {
        it('should reset update flag on updateUserData', () => {
            const action = updateUserData({ req: { email: 'test@example.com', oldPassword: 'oldPass', password: 'newPass' } });
            const state = authReducer(initialState, action);

            expect(state.isUpdateSuccessful).toBe(false);
            expect(state.error).toBeNull();
        });

        it('should update authData and set update flag on updateUserDataSuccess', () => {
            const updateRequest = { email: 'updated@example.com', oldPassword: '', password: '' };
            const action = updateUserDataSuccess({ req: updateRequest });
            const state = authReducer(initialState, action);

            const updatedAuthData = copyUserUpdateRequestToUserAuth(initialState.authData, updateRequest);

            expect(state.isUpdateSuccessful).toBe(true);
            expect(state.authData).toEqual(updatedAuthData);
        });

        it('should set error on updateUserDataFailure', () => {
            const error = 'Update failed';
            const action = updateUserDataFailure({ error });
            const state = authReducer(initialState, action);

            expect(state.error).toEqual(error);
        });
    });
});
