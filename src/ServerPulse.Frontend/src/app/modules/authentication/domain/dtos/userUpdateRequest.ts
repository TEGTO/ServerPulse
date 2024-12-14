import { AuthData } from "../..";

export interface UserUpdateRequest {
    email: string;
    oldPassword: string;
    password: string;
}

export function copyUserUpdateRequestToUserAuth(authData: AuthData, req: UserUpdateRequest): AuthData {
    return {
        ...authData,
        email: req.email,
    }
}