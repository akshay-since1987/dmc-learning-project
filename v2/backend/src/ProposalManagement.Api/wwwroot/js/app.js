// App entry point — register routes and initialize
import { registerRoute, initRouter } from './router.js';
import { renderLayout, clearLayout } from './layout.js';
import { isAuthenticated } from './auth.js';

// Pages
import { renderLoginPage } from './pages/login.js';
import { renderDashboardPage } from './pages/dashboard.js';
import { renderProposalListPage } from './pages/proposal-list.js';
import { renderProposalDetailPage } from './pages/proposal-detail.js';
import { renderProposalFormPage } from './pages/proposal-form.js';
import { renderAdminUsersPage } from './pages/admin-users.js';
import { renderAdminMastersPage } from './pages/admin-masters.js';
import { renderAuditTrailPage } from './pages/audit-trail.js';

// ── Register routes ──

registerRoute('/login', async () => {
    clearLayout();
    await renderLoginPage();
});

registerRoute('/dashboard', async () => {
    renderLayout();
    await renderDashboardPage();
});

registerRoute('/', async () => {
    window.location.hash = '#/dashboard';
});

// Proposal lists
registerRoute('/proposals/my', async () => {
    renderLayout();
    await renderProposalListPage({ mode: 'my' });
});

registerRoute('/proposals/all', async () => {
    renderLayout();
    await renderProposalListPage({ mode: 'all' });
});

registerRoute('/proposals/pending', async () => {
    renderLayout();
    await renderProposalListPage({ mode: 'pending' });
});

registerRoute('/approvals', async () => {
    renderLayout();
    await renderProposalListPage({ mode: 'pending' });
});

// Proposal form
registerRoute('/proposals/new', async () => {
    renderLayout();
    await renderProposalFormPage();
});

registerRoute('/proposals/:id/edit', async (params) => {
    renderLayout();
    await renderProposalFormPage(params);
});

// Proposal detail
registerRoute('/proposals/:id', async (params) => {
    renderLayout();
    await renderProposalDetailPage(params);
});

// Admin — Lotus only
registerRoute('/admin/users', async () => {
    renderLayout();
    await renderAdminUsersPage();
});

registerRoute('/admin/masters', async () => {
    renderLayout();
    await renderAdminMastersPage();
});

// Audit trail — Lotus, Commissioner, Auditor
registerRoute('/audit', async () => {
    renderLayout();
    await renderAuditTrailPage();
});

// ── Initialize ──
if (!isAuthenticated() && !window.location.hash.includes('/login')) {
    window.location.hash = '#/login';
}

initRouter();
