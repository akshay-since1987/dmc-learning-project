/** Centralised API Client — wraps fetch() with JWT, error handling, base URL */

const BASE_URL = '/api';

class ApiClient {
  constructor() {
    this._baseUrl = BASE_URL;
  }

  /** Get the stored JWT access token */
  _getToken() {
    return localStorage.getItem('accessToken');
  }

  /** Build headers with optional auth */
  _headers(contentType = 'application/json') {
    const h = {};
    if (contentType) h['Content-Type'] = contentType;
    const token = this._getToken();
    if (token) h['Authorization'] = `Bearer ${token}`;
    return h;
  }

  /** Core request method */
  async _request(method, path, body = null, options = {}) {
    const url = `${this._baseUrl}${path}`;
    const config = {
      method,
      headers: this._headers(options.contentType),
    };

    if (body !== null && body !== undefined) {
      if (body instanceof FormData) {
        delete config.headers['Content-Type']; // let browser set boundary
        config.body = body;
      } else {
        config.body = JSON.stringify(body);
      }
    }

    let response;
    try {
      response = await fetch(url, config);
    } catch (err) {
      throw { status: 0, message: 'Network error. Please check your connection.', errors: [] };
    }

    // Handle 401 — try refresh once (skip for auth endpoints — they return 401 for invalid credentials)
    const isAuthEndpoint = path.startsWith('/auth/');
    if (response.status === 401 && !options._isRetry && !isAuthEndpoint) {
      const refreshed = await this._tryRefresh();
      if (refreshed) {
        return this._request(method, path, body, { ...options, _isRetry: true });
      }
      // Refresh failed — force logout
      window.dispatchEvent(new CustomEvent('auth:logout'));
      throw { status: 401, message: 'Session expired. Please login again.', errors: [] };
    }

    // Parse response
    let data = null;
    const ct = response.headers.get('content-type') || '';
    if (ct.includes('application/json')) {
      data = await response.json();
    } else if (response.status !== 204) {
      data = await response.text();
    }

    if (!response.ok) {
      const error = {
        status: response.status,
        message: data?.message || data?.title || `Request failed (${response.status})`,
        errors: data?.errors || [],
      };
      throw error;
    }

    return data;
  }

  /** Try to refresh the access token using the refresh token */
  async _tryRefresh() {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) return false;

    try {
      const response = await fetch(`${this._baseUrl}/auth/refresh-token`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken }),
      });

      if (!response.ok) return false;

      const data = await response.json();
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      if (data.user) {
        localStorage.setItem('currentUser', JSON.stringify(data.user));
      }
      return true;
    } catch {
      return false;
    }
  }

  /** GET request */
  get(path) {
    return this._request('GET', path);
  }

  /** POST request */
  post(path, body) {
    return this._request('POST', path, body);
  }

  /** PUT request */
  put(path, body) {
    return this._request('PUT', path, body);
  }

  /** DELETE request */
  delete(path) {
    return this._request('DELETE', path);
  }

  /** Upload files via FormData */
  upload(path, formData) {
    return this._request('POST', path, formData, { contentType: null });
  }
}

/** Singleton API client instance */
const api = new ApiClient();
export default api;
