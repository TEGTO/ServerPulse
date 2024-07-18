import { MemoizedSelector, createFeatureSelector, createSelector } from "@ngrx/store";
import { AuthState, RegistrationState, UserDataState } from "../..";
import { AuthData, UserData } from "../../../shared";

//Registration
export const selectRegistrationState = createFeatureSelector<RegistrationState>('registration');
export const selectIsRegistrationSuccess = createSelector(
    selectRegistrationState,
    (state: RegistrationState) => state.isSuccess
);
export const selectRegistrationErrors = createSelector(
    selectRegistrationState,
    (state: RegistrationState) => state.error
);
//Auth
export const selectAuthState = createFeatureSelector<AuthState>('authentication');
export const selectAuthData: MemoizedSelector<object, AuthData> = createSelector(
    selectAuthState,
    (state: AuthState) => ({
        isAuthenticated: state.isAuthenticated,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        refreshTokenExpiryDate: state.refreshTokenExpiryDate
    })
);
export const selectAuthErrors = createSelector(
    selectAuthState,
    (state: AuthState) => state.error
);
//User Data
export const selectUserDataState = createFeatureSelector<UserDataState>('userdata');
export const selectUserData: MemoizedSelector<object, UserData> = createSelector(
    selectUserDataState,
    (state: UserData) => ({
        userName: state.userName,
        email: state.email
    })
);
export const selectIsUpdateSuccessful = createSelector(
    selectUserDataState,
    (state: UserDataState) => state.isUpdateSuccess
);
export const selectUserErrors = createSelector(
    selectUserDataState,
    (state: UserDataState) => state.error
);