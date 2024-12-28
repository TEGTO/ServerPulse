export function getFullEmailConfirmRedirectPath() {
    return `${window.location.origin}/auth/callback`;
}

export function getEmailConfirmRedirectPath() {
    return `auth/callback`;
}

export function getFullOAuthRedirectPath() {
    return `${window.location.origin}/oauth/callback`;
}

export function getOAuthRedirectPath() {
    return `oauth/callback`;
}