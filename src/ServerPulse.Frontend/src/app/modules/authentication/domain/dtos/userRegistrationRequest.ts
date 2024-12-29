export interface UserRegistrationRequest {
    redirectConfirmUrl: string;
    email: string;
    password: string;
    confirmPassword: string;
}