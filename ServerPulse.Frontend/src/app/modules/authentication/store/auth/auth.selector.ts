import { MemoizedSelector, createFeatureSelector, createSelector } from "@ngrx/store";
import { AuthState, RegistrationState } from "../..";
import { UserAuthData } from "../../../shared";

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
export const selectAuthData: MemoizedSelector<object, UserAuthData> = createSelector(
    selectAuthState,
    (state: AuthState) => ({
        isAuthenticated: state.isAuthenticated,
        authToken: state.authToken,
        refreshToken: state.refreshToken,
        refreshTokenExpiryDate: state.refreshTokenExpiryDate,
        userEmail: state.userEmail
    })
);
export const selectAuthErrors = createSelector(
    selectAuthState,
    (state: AuthState) => state.error
);
export const selectUpdateIsSuccessful = createSelector(
    selectAuthState,
    (state: AuthState) => state.error == null
);