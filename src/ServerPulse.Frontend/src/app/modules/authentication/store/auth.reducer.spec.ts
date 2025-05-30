/* eslint-disable @typescript-eslint/no-explicit-any */
import { AuthData, authFailure, authReducer, AuthState, copyUserUpdateRequestToUserAuth, getAuthData, getAuthDataFailure, getAuthDataSuccess, getDefaultAccessTokenData, getDefaultAuthData, loginUser, loginUserSuccess, logOutUserSuccess, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerUser, startLoginUser, startRegisterUser, updateUserData, updateUserDataSuccess } from "..";

describe('Auth Reducer', () => {
    const initialState: AuthState = {
        isRefreshSuccessful: false,
        authData: getDefaultAuthData(),
        error: null,
    };

    it('should return the initial state by default', () => {
        const action = { type: 'UNKNOWN' } as any;
        const state = authReducer(initialState, action);

        expect(state).toEqual(initialState);
    });

    it('should set error on authFailure', () => {
        const error = 'Login failed';
        const action = authFailure({ error: error });
        const state = authReducer(initialState, action);

        expect(state.error).toEqual(error);
    });

    describe('Registration Actions', () => {
        it('should reset state on startRegisterUser', () => {
            const action = startRegisterUser();
            const state = authReducer(initialState, action);

            expect(state).toEqual(initialState);
        });

        it('should reset state on registerUser', () => {
            const action = registerUser({ req: { redirectConfirmUrl: 'some-url', email: 'test@example.com', password: 'password', confirmPassword: 'password' } });
            const state = authReducer(initialState, action);

            expect(state.authData.email).toEqual("");
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

            expect(state.authData.email).toEqual("");
        });

        it('should update authData on loginUserSuccess', () => {
            const authData: AuthData = { isAuthenticated: true, accessTokenData: getDefaultAccessTokenData(), email: 'test@example.com' };
            const action = loginUserSuccess({ authData });
            const state = authReducer(initialState, action);

            expect(state.authData).toEqual(authData);
            expect(state.error).toBeNull();
        });
    });

    describe('Auth Data Actions', () => {
        it('should reset state on getAuthData', () => {
            const action = getAuthData();
            const state = authReducer(initialState, action);

            expect(state).toEqual(initialState);
        });

        it('should update authData on getAuthDataSuccess', () => {
            const authData: AuthData = { isAuthenticated: true, accessTokenData: getDefaultAccessTokenData(), email: 'test@example.com' };
            const action = getAuthDataSuccess({ authData });
            const state = authReducer(initialState, action);

            expect(state.authData).toEqual(authData);
            expect(state.error).toBeNull();
        });

        it('should reset state on getAuthDataFailure', () => {
            const action = getAuthDataFailure();
            const state = authReducer(initialState, action);

            expect(state.authData.email).toEqual("");
        });
    });

    describe('Logout Actions', () => {
        it('should reset state on logOutUserSuccess', () => {
            const action = logOutUserSuccess();
            const state = authReducer(initialState, action);

            expect(state.authData.email).toEqual("");
        });
    });

    describe('Refresh Token Actions', () => {
        it('should set refresh flag on refreshAccessToken', () => {
            const accessTokenData = { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() };
            const action = refreshAccessToken({ accessTokenData });
            const state = authReducer(initialState, action);

            expect(state.isRefreshSuccessful).toBe(false);
            expect(state.error).toBeNull();
        });

        it('should update authData and set refresh flag on refreshAccessTokenSuccess', () => {
            const accessTokenData = { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() };
            const action = refreshAccessTokenSuccess({ accessTokenData });
            const state = authReducer(initialState, action);

            expect(state.isRefreshSuccessful).toBe(true);
            expect(state.authData.accessTokenData).toEqual(accessTokenData);
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

            expect(state.error).toBeNull();
        });

        it('should update authData and set update flag on updateUserDataSuccess', () => {
            const updateRequest = { email: 'updated@example.com', oldPassword: '', password: '' };
            const action = updateUserDataSuccess({ req: updateRequest });
            const state = authReducer(initialState, action);

            const updatedAuthData = copyUserUpdateRequestToUserAuth(initialState.authData, updateRequest);

            expect(state.authData).toEqual(updatedAuthData);
        });
    });
});
