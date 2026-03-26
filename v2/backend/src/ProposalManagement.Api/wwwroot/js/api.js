// Centralized API client with JWT injection
const BASE_URL = '/api';

function getToken() {
    return localStorage.getItem('accessToken');
}

function getRefreshToken() {
    return localStorage.getItem('refreshToken');
}

function setTokens(access, refresh) {
    localStorage.setItem('accessToken', access);
    localStorage.setItem('refreshToken', refresh);
}

function clearTokens() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
}

async function refreshAccessToken() {
    const rt = getRefreshToken();
    if (!rt) return false;

    try {
        const res = await fetch(`${BASE_URL}/auth/refresh-token`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken: rt })
        });
        if (!res.ok) return false;
        const json = await res.json();
        if (json.success && json.data) {
            setTokens(json.data.accessToken, json.data.refreshToken);
            return true;
        }
    } catch { /* ignore */ }
    return false;
}

async function request(path, options = {}) {
    const url = `${BASE_URL}${path}`;
    const headers = { ...options.headers };

    const token = getToken();
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    if (options.body && !(options.body instanceof FormData)) {
        headers['Content-Type'] = 'application/json';
        if (typeof options.body === 'object') {
            options.body = JSON.stringify(options.body);
        }
    }

    let res = await fetch(url, { ...options, headers });

    // Auto-refresh on 401
    if (res.status === 401 && getRefreshToken()) {
        const refreshed = await refreshAccessToken();
        if (refreshed) {
            headers['Authorization'] = `Bearer ${getToken()}`;
            res = await fetch(url, { ...options, headers });
        } else {
            clearTokens();
            window.location.hash = '#/login';
            return { success: false, error: 'Session expired' };
        }
    }

    if (res.status === 204) return { success: true };

    try {
        return await res.json();
    } catch {
        return { success: false, error: 'Network error' };
    }
}

export const api = {
    get: (path) => request(path, { method: 'GET' }),
    post: (path, body) => request(path, { method: 'POST', body }),
    put: (path, body) => request(path, { method: 'PUT', body }),
    delete: (path) => request(path, { method: 'DELETE' }),
    upload: (path, formData) => request(path, { method: 'POST', body: formData }),
    getToken,
    setTokens,
    clearTokens
};
