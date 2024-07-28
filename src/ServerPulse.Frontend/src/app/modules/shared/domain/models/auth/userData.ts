import { UserAuthenticationResponse } from "../../dtos/auth/userAuthenticationResponse";
import { UserUpdateDataRequest } from "../../dtos/auth/userUpdateDataRequest";

export interface UserData {
    userName: string;
    email: string;
}
export function getUserFromAuthResponse(response: UserAuthenticationResponse): UserData {
    return {
        userName: response.userName,
        email: response.email,
    }
}
export function getUserFromUpdateRequest(request: UserUpdateDataRequest): UserData {
    return {
        userName: request.userName,
        email: request.newEmail
            ? request.newEmail
            : request.oldEmail
    }
}