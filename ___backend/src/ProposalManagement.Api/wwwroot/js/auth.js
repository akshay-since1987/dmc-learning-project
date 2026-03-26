/** Auth module — OTP login, JWT storage, role-based guards */

import api from './api.js';
import { showToast } from './toast.js';

const TOKEN_KEY = 'accessToken';
const REFRESH_KEY = 'refreshToken';
const USER_KEY = 'currentUser';

/** Store current user in local storage */
export function setCurrentUser(user) {
  localStorage.setItem(USER_KEY, JSON.stringify(user || null));
}

/** Get current stored user */
export function getCurrentUser() {
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try { return JSON.parse(raw); } catch { return null; }
}

/** Check if user is logged in (has a token) */
export function isAuthenticated() {
  return !!localStorage.getItem(TOKEN_KEY);
}

/** Get the current user's role */
export function getUserRole() {
  return getCurrentUser()?.role || null;
}

/** Check if user has one of the allowed roles */
export function hasRole(...roles) {
  const r = getUserRole();
  return r && roles.includes(r);
}

/** Send OTP to mobile number */
export async function sendOtp(mobileNumber) {
  return api.post('/auth/send-otp', { mobileNumber });
}

/** Verify OTP (+ password for Lotus) and store tokens */
export async function verifyOtp(mobileNumber, otp, password = null) {
  const payload = { mobileNumber, otp };
  if (password) payload.password = password;

  const data = await api.post('/auth/verify-otp', payload);

  // Store tokens and user info
  localStorage.setItem(TOKEN_KEY, data.accessToken);
  localStorage.setItem(REFRESH_KEY, data.refreshToken);
  setCurrentUser(data.user);

  return data;
}

/** Logout — clear tokens and redirect */
export function logout() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_KEY);
  localStorage.removeItem(USER_KEY);
  window.location.hash = '#/login';
}

/** Listen for forced logout (from API client on 401) */
window.addEventListener('auth:logout', () => {
  logout();
  showToast('Session expired. Please login again.', 'warning');
});

export default {
  setCurrentUser,
  getCurrentUser,
  isAuthenticated,
  getUserRole,
  hasRole,
  sendOtp,
  verifyOtp,
  logout,
};
