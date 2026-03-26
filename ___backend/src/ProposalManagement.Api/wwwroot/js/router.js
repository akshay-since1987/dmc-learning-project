/** Simple hash-based SPA router */

import { isAuthenticated, getUserRole } from './auth.js';

/** @type {{ path: string, handler: () => Promise<void>|void, roles?: string[] }[]} */
const routes = [];

/** Currently active route path */
let currentPath = '';

/** Register a route */
export function addRoute(path, handler, roles = null) {
  routes.push({ path, handler, roles });
}

/** Navigate to a hash path programmatically */
export function navigate(path) {
  window.location.hash = path;
}

/** Get the current hash path */
export function getCurrentPath() {
  return window.location.hash.slice(1) || '/dashboard';
}

/** Match a route pattern (supports :param placeholders) */
function matchRoute(pattern, path) {
  const patternParts = pattern.split('/');
  const pathParts = path.split('/');

  if (patternParts.length !== pathParts.length) return null;

  const params = {};
  for (let i = 0; i < patternParts.length; i++) {
    if (patternParts[i].startsWith(':')) {
      params[patternParts[i].slice(1)] = decodeURIComponent(pathParts[i]);
    } else if (patternParts[i] !== pathParts[i]) {
      return null;
    }
  }
  return params;
}

/** Handle route change */
async function handleRouteChange() {
  const path = getCurrentPath();
  if (path === currentPath) return;
  currentPath = path;

  // Public routes (no auth required)
  if (path === '/login') {
    if (isAuthenticated()) {
      navigate('/dashboard');
      return;
    }
    const matched = routes.find(r => r.path === '/login');
    if (matched) await matched.handler({});
    return;
  }

  // Auth guard
  if (!isAuthenticated()) {
    navigate('/login');
    return;
  }

  // Find matching route
  for (const route of routes) {
    const params = matchRoute(route.path, path);
    if (params !== null) {
      // Role guard
      if (route.roles && route.roles.length > 0) {
        const role = getUserRole();
        if (!route.roles.includes(role)) {
          navigate('/dashboard');
          return;
        }
      }
      await route.handler(params);
      return;
    }
  }

  // 404 — redirect to dashboard
  navigate('/dashboard');
}

/** Initialize the router */
export function initRouter() {
  window.addEventListener('hashchange', handleRouteChange);
  // Handle initial load
  if (!window.location.hash) {
    window.location.hash = isAuthenticated() ? '#/dashboard' : '#/login';
  } else {
    handleRouteChange();
  }
}

export default { addRoute, navigate, getCurrentPath, initRouter };
