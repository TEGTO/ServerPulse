import { AuthToken } from "../..";

export interface UserAuthenticationResponse {
    authToken: AuthToken;
    userName: string;
    email: string;
}