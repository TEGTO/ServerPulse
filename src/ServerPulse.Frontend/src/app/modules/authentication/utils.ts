export function getFullOAuthRedirectPath() {
    return `${window.location.origin}/auth/callback`;
}

export function getOAuthRedirectPath() {
    return `auth/callback`;
}