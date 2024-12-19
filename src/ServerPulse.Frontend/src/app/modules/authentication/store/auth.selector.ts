import { MemoizedSelector, createFeatureSelector, createSelector } from "@ngrx/store";
import { AuthData, AuthState } from "..";

export const selectAuthState = createFeatureSelector<AuthState>('authentication');
export const selectAuthData: MemoizedSelector<object, AuthData> = createSelector(
    selectAuthState,
    (state: AuthState) => state.authData
);
export const selectIsRegistrationSuccessful = createSelector(
    selectAuthState,
    (state: AuthState) => state.isRefreshSuccessful
);
export const selectIsRefreshSuccessful = createSelector(
    selectAuthState,
    (state: AuthState) => state.isRefreshSuccessful
);
export const selectIsUpdateSuccessful = createSelector(
    selectAuthState,
    (state: AuthState) => state.isUpdateSuccessful
);
export const selectAuthErrors = createSelector(
    selectAuthState,
    (state: AuthState) => state.error
);