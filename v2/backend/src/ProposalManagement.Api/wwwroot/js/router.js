// Simple hash-based router
import { isAuthenticated } from './auth.js';

const routes = {};
let currentCleanup = null;

export function registerRoute(path, handler) {
    routes[path] = handler;
}

export function navigate(path) {
    window.location.hash = `#${path}`;
}

function getRoutePath() {
    const hash = window.location.hash.slice(1) || '/';
    return hash;
}

function findRoute(path) {
    // Exact match first
    if (routes[path]) return { handler: routes[path], params: {} };

    // Pattern match (e.g., /proposals/:id)
    for (const [pattern, handler] of Object.entries(routes)) {
        const patternParts = pattern.split('/');
        const pathParts = path.split('/');
        if (patternParts.length !== pathParts.length) continue;

        const params = {};
        let match = true;
        for (let i = 0; i < patternParts.length; i++) {
            if (patternParts[i].startsWith(':')) {
                params[patternParts[i].slice(1)] = pathParts[i];
            } else if (patternParts[i] !== pathParts[i]) {
                match = false;
                break;
            }
        }
        if (match) return { handler, params };
    }
    return null;
}

async function handleRoute() {
    const path = getRoutePath();

    // Auth guard — allow /login without auth
    if (path !== '/login' && !isAuthenticated()) {
        window.location.hash = '#/login';
        return;
    }

    // If authenticated and going to login, redirect to dashboard
    if (path === '/login' && isAuthenticated()) {
        window.location.hash = '#/dashboard';
        return;
    }

    // Clean up previous page
    if (currentCleanup) {
        currentCleanup();
        currentCleanup = null;
    }

    const route = findRoute(path);
    if (route) {
        const cleanup = await route.handler(route.params);
        if (typeof cleanup === 'function') {
            currentCleanup = cleanup;
        }
    } else {
        document.getElementById('page-content').innerHTML = `
            <div class="text-center py-5">
                <h2 class="text-muted">404 — Page Not Found</h2>
                <p>The page you're looking for doesn't exist.</p>
                <a href="#/dashboard" class="btn btn-primary">Go to Dashboard</a>
            </div>`;
    }
}

export function initRouter() {
    window.addEventListener('hashchange', handleRoute);
    handleRoute();
}
