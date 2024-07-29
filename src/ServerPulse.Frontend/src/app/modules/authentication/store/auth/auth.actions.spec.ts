import { AuthData, AuthToken, UserAuthenticationRequest, UserData, UserRegistrationRequest, UserUpdateDataRequest } from "../../../shared";
import { getAuthData, getAuthDataFailure, getAuthDataSuccess, logOutUser, logOutUserSuccess, refreshAccessToken, refreshAccessTokenFailure, refreshAccessTokenSuccess, registerUser, registerFailure as registerUserFailure, registerSuccess as registerUserSuccess, signInUser, signInUserFailure, signInUserSuccess, updateUserData, updateUserDataFailure, updateUserDataSuccess } from "./auth.actions";

describe('Registration Actions', () => {
    const error = { message: 'An error occurred' };

    describe('Register User Actions', () => {
        it('should create registerUser action', () => {
            const req: UserRegistrationRequest =
            {
                userName: "userName",
                email: "email",
                password: "password",
                confirmPassword: "confirmPassword"
            }
            const action = registerUser({ registrationRequest: req });
            expect(action.type).toBe('[Registration] Register New User');
            expect(action.registrationRequest).toBe(req);
        });
        it('should create registerUserSuccess action', () => {
            const action = registerUserSuccess();
            expect(action.type).toBe('[Registration] Register New User Success');
        });
        it('should create registerUserFailure action', () => {
            const action = registerUserFailure({ error });
            expect(action.type).toBe('[Registration] Register New User Failure');
            expect(action.error).toEqual(error);
        });
    });
});

describe('Authentication Actions', () => {
    const error = { message: 'An error occurred' };

    describe('Sign In Actions', () => {
        it('should create signInUser action', () => {
            const req: UserAuthenticationRequest =
            {
                login: "login",
                password: "password",
            }
            const action = signInUser({ authRequest: req });
            expect(action.type).toBe('[Auth] Sing In By User Data');
            expect(action.authRequest).toBe(req);
        });
        it('should create signInUserSuccess action', () => {
            const authData: AuthData =
            {
                isAuthenticated: true,
                accessToken: "authToken",
                refreshToken: "refreshToken",
                refreshTokenExpiryDate: new Date()
            };
            const userData: UserData =
            {
                userName: "userName",
                email: "email"
            };
            const action = signInUserSuccess({ authData: authData, userData: userData });
            expect(action.type).toBe('[Auth] Sing In By User Data Success');
            expect(action.authData).toBe(authData);
            expect(action.userData).toBe(userData);
        });
        it('should create signInUserFailure action', () => {
            const action = signInUserFailure({ error });
            expect(action.type).toBe('[Auth] Sing In By User Data Failure');
            expect(action.error).toEqual(error);
        });
    });

    describe('Get Authentication Data Actions', () => {
        it('should create getAuthData action', () => {
            const action = getAuthData();
            expect(action.type).toBe('[Auth] Get Authenticated Data');
        });
        it('should create signInUserSuccess action', () => {
            const authData: AuthData =
            {
                isAuthenticated: true,
                accessToken: "authToken",
                refreshToken: "refreshToken",
                refreshTokenExpiryDate: new Date()
            };
            const userData: UserData =
            {
                userName: "userName",
                email: "email"
            };
            const action = getAuthDataSuccess({ authData: authData, userData: userData });
            expect(action.type).toBe('[Auth] Get Authenticated Data Success');
            expect(action.authData).toBe(authData);
            expect(action.userData).toBe(userData);
        });
        it('should create signInUserFailure action', () => {
            const action = getAuthDataFailure();
            expect(action.type).toBe('[Auth] Get Authenticated Data Failure');
        });

        describe('Log Out User Actions', () => {
            it('should create logOutUser action', () => {
                const action = logOutUser();
                expect(action.type).toBe('[Auth] Log out Authenticated User');
            });
            it('should create signInUserSuccess action', () => {
                const action = logOutUserSuccess();
                expect(action.type).toBe('[Auth] Log out Authenticated User Success');
            });
        });

        describe('Refresh Access Token Actions', () => {
            it('should create refreshAccessToken action', () => {
                const accessToken: AuthToken =
                {
                    accessToken: "accessToken",
                    refreshToken: "refreshToken",
                    refreshTokenExpiryDate: new Date()
                }
                const action = refreshAccessToken({ authToken: accessToken });
                expect(action.type).toBe('[Auth] Refresh Access Token');
                expect(action.authToken).toBe(accessToken);
            });
            it('should create refreshAccessTokenSuccess action', () => {
                const accessToken: AuthToken =
                {
                    accessToken: "accessToken",
                    refreshToken: "refreshToken",
                    refreshTokenExpiryDate: new Date()
                }
                const action = refreshAccessTokenSuccess({ authData: accessToken });
                expect(action.type).toBe('[Auth] Refresh Access Token Success');
                expect(action.authData).toBe(accessToken);
            });
            it('should create refreshAccessTokenFailure action', () => {
                const action = refreshAccessTokenFailure({ error });
                expect(action.type).toBe('[Auth] Refresh Access Token Failure');
                expect(action.error).toEqual(error);
            });
        });

        describe('Update User Actions', () => {
            it('should create updateUserData action', () => {
                const req: UserUpdateDataRequest =
                {
                    userName: "userName",
                    oldEmail: "oldEmail",
                    newEmail: "newEmail",
                    oldPassword: "oldPassword",
                    newPassword: "newPassword"
                }
                const action = updateUserData({ updateRequest: req });
                expect(action.type).toBe('[Auth] Update Authenticated User');
                expect(action.updateRequest).toBe(req);
            });
            it('should create updateUserDataSuccess action', () => {
                const userData: UserData =
                {
                    userName: "userName",
                    email: "email"
                };
                const action = updateUserDataSuccess({ userData: userData });
                expect(action.type).toBe('[Auth] Update Authenticated User Success');
                expect(action.userData).toBe(userData);
            });
            it('should create updateUserDataFailure action', () => {
                const action = updateUserDataFailure({ error });
                expect(action.type).toBe('[Auth] Update Authenticated User Failure');
                expect(action.error).toEqual(error);
            });
        });
    });
});