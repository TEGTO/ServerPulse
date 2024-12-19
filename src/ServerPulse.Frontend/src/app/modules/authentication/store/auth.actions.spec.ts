import { AuthData, AuthToken, getAuthData, getAuthDataFailure, getAuthDataSuccess, loginUser, loginUserFailure, loginUserSuccess, logOutUser, logOutUserSuccess, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerFailure, registerSuccess, registerUser, startLoginUser, startRegisterUser, updateUserData, updateUserDataFailure, updateUserDataSuccess, UserAuthenticationRequest, UserRegistrationRequest, UserUpdateRequest } from "..";

describe('Authentication Actions', () => {
    const error = { message: 'An error occurred' };

    describe('Register User Actions', () => {
        it('should create startRegisterUser action', () => {
            const action = startRegisterUser();
            expect(action.type).toBe('[Auth] Start Register New User');
        });

        it('should create registerUser action', () => {
            const req: UserRegistrationRequest = {
                email: 'email@example.com',
                password: 'password123',
                confirmPassword: 'password123',
            };
            const action = registerUser({ req });
            expect(action.type).toBe('[Auth] Register New User');
            expect(action.req).toBe(req);
        });

        it('should create registerSuccess action', () => {
            const authData: AuthData = {
                isAuthenticated: true,
                authToken: { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() },
                email: 'email@example.com',
            };
            const action = registerSuccess({ authData });
            expect(action.type).toBe('[Auth] Register New User Success');
            expect(action.authData).toBe(authData);
        });

        it('should create registerFailure action', () => {
            const action = registerFailure({ error });
            expect(action.type).toBe('[Auth] Register New User Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Login User Actions', () => {
        it('should create startLoginUser action', () => {
            const action = startLoginUser();
            expect(action.type).toBe('[Auth] Start Login User');
        });

        it('should create loginUser action', () => {
            const req: UserAuthenticationRequest = { login: 'email@example.com', password: 'password123' };
            const action = loginUser({ req });
            expect(action.type).toBe('[Auth] Login By User');
            expect(action.req).toBe(req);
        });

        it('should create loginUserSuccess action', () => {
            const authData: AuthData = {
                isAuthenticated: true,
                authToken: { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() },
                email: 'email@example.com',
            };
            const action = loginUserSuccess({ authData });
            expect(action.type).toBe('[Auth] Login By User Success');
            expect(action.authData).toBe(authData);
        });

        it('should create loginUserFailure action', () => {
            const action = loginUserFailure({ error });
            expect(action.type).toBe('[Auth] Login By User Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Authentication Data Actions', () => {
        it('should create getAuthData action', () => {
            const action = getAuthData();
            expect(action.type).toBe('[Auth] Get Authenticated Data');
        });

        it('should create getAuthDataSuccess action', () => {
            const authData: AuthData = {
                isAuthenticated: true,
                authToken: { accessToken: 'token', refreshToken: 'refreshToken', refreshTokenExpiryDate: new Date() },
                email: 'email@example.com',
            };
            const action = getAuthDataSuccess({ authData });
            expect(action.type).toBe('[Auth] Get Authenticated Data Success');
            expect(action.authData).toBe(authData);
        });

        it('should create getAuthDataFailure action', () => {
            const action = getAuthDataFailure();
            expect(action.type).toBe('[Auth] Get Authenticated Data Failure');
        });
    });

    describe('Log Out User Actions', () => {
        it('should create logOutUser action', () => {
            const action = logOutUser();
            expect(action.type).toBe('[Auth] Log out Authenticated User');
        });

        it('should create logOutUserSuccess action', () => {
            const action = logOutUserSuccess();
            expect(action.type).toBe('[Auth] Log out Authenticated User Success');
        });
    });

    describe('Refresh Access Token Actions', () => {
        it('should create refreshAccessToken action', () => {
            const authToken: AuthToken = {
                accessToken: 'accessToken123',
                refreshToken: 'refreshToken123',
                refreshTokenExpiryDate: new Date(),
            };
            const action = refreshAccessToken({ authToken });
            expect(action.type).toBe('[Auth] Refresh Access Token');
            expect(action.authToken).toBe(authToken);
        });

        it('should create refreshAccessTokenSuccess action', () => {
            const authToken: AuthToken = {
                accessToken: 'accessToken123',
                refreshToken: 'refreshToken123',
                refreshTokenExpiryDate: new Date(),
            };
            const action = refreshAccessTokenSuccess({ authToken });
            expect(action.type).toBe('[Auth] Refresh Access Token Success');
            expect(action.authToken).toBe(authToken);
        });

        it('should create refreshAccessTokenFailure action', () => {
            const action = refreshAccessTokenFailure({ error });
            expect(action.type).toBe('[Auth] Refresh Access Token Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Update User Actions', () => {
        it('should create updateUserData action', () => {
            const req: UserUpdateRequest = {
                email: 'email@example.com',
                oldPassword: 'oldPassword123',
                password: 'newPassword123',
            };
            const action = updateUserData({ req });
            expect(action.type).toBe('[Auth] Update User Data');
            expect(action.req).toBe(req);
        });

        it('should create updateUserDataSuccess action', () => {
            const req: UserUpdateRequest = {
                email: 'email@example.com',
                oldPassword: 'oldPassword123',
                password: 'newPassword123',
            };
            const action = updateUserDataSuccess({ req });
            expect(action.type).toBe('[Auth] Update User Data Success');
            expect(action.req).toBe(req);
        });

        it('should create updateUserDataFailure action', () => {
            const action = updateUserDataFailure({ error });
            expect(action.type).toBe('[Auth] Update User Data Failure');
            expect(action.error).toEqual(error);
        });
    });
});
