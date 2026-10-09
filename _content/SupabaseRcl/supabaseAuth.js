export function saveSession(sessionJson) {
    if (sessionJson) {
        localStorage.setItem("sb_session", sessionJson);
    } else {
        localStorage.removeItem("sb_session");
    }
}

export function getSession() {
    return localStorage.getItem("sb_session");
}

export function clearSession() {
    localStorage.removeItem("sb_session");
}
