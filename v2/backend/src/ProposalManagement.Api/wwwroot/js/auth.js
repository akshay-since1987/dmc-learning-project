// Auth module — token management, login/logout, user state
import { api } from './api.js';

export function getUser() {
    const raw = localStorage.getItem('user');
    return raw ? JSON.parse(raw) : null;
}

export function setUser(user) {
    localStorage.setItem('user', JSON.stringify(user));
}

export function isAuthenticated() {
    return !!api.getToken() && !!getUser();
}

export function requireAuth() {
    if (!isAuthenticated()) {
        window.location.hash = '#/login';
        return false;
    }
    return true;
}

export function hasRole(...roles) {
    const user = getUser();
    return user && roles.includes(user.role);
}

export async function login(mobileNumber, otp) {
    const res = await api.post('/auth/verify-otp', { mobileNumber, otp });
    if (res.success && res.data) {
        api.setTokens(res.data.accessToken, res.data.refreshToken);
        setUser({
            id: res.data.userId,
            fullName: res.data.fullName,
            role: res.data.role,
            palikaId: res.data.palikaId
        });
    }
    return res;
}

export async function sendOtp(mobileNumber) {
    return api.post('/auth/send-otp', { mobileNumber });
}

export function logout() {
    api.clearTokens();
    localStorage.removeItem('user');
    window.location.hash = '#/login';
}

export async function fetchProfile() {
    const res = await api.get('/auth/me');
    if (res.success && res.data) {
        setUser({
            id: res.data.id,
            fullName: res.data.fullName_En,
            role: res.data.role,
            palikaId: res.data.palikaId,
            departmentName: res.data.departmentName,
            designationName: res.data.designationName
        });
    }
    return res;
}
