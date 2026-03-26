/** App entry point — initialises router, loads pages, manages layout */

import { addRoute, initRouter, navigate } from './router.js';
import { initI18n, setLang, getLang, translateDOM } from './i18n.js';
import { isAuthenticated, getCurrentUser, logout, hasRole } from './auth.js';
import { showToast } from './toast.js';
import { $, $$, getInitials } from './utils.js';

// ── Layout management ──────────────────────────────────────────────
const appEl = $('app');

/** Show the full app layout (sidebar + header + content area) */
function showAppLayout() {
  const user = getCurrentUser();
  const role = user?.role || '';
  const initials = getInitials(user?.fullName_En || 'U');
  const lang = getLang();

  appEl.innerHTML = `
    <a href="#main-content" class="skip-link">Skip to main content</a>
    <div class="app-wrapper">
      <!-- Sidebar -->
      <nav class="sidebar" id="sidebar" aria-label="Main navigation">
        <a class="sidebar-brand" href="#/dashboard">
          <div class="sidebar-brand-text">
            <span data-i18n="app.title">Dhule Municipal Corporation</span>
            <small data-i18n="app.subtitle">Administrative Approval Office Note</small>
          </div>
        </a>
        <ul class="sidebar-nav" id="sidebar-nav"></ul>
        <div class="sidebar-footer">
          <small>© 2025 DMC</small>
        </div>
      </nav>

      <!-- Sidebar overlay (mobile) -->
      <div class="sidebar-overlay" id="sidebar-overlay"></div>

      <!-- Header -->
      <header class="app-header">
        <div class="header-left">
          <button class="sidebar-toggle" id="sidebar-toggle" aria-label="Toggle sidebar">
            <i class="bi bi-list"></i>
          </button>
          <h1 class="header-page-title" id="page-title"></h1>
        </div>
        <div class="header-right">
          <!-- Notification bell -->
          <div class="position-relative me-2" id="notification-bell-wrapper" style="cursor:pointer" role="button" tabindex="0" aria-label="Notifications">
            <i class="bi bi-bell fs-5"></i>
            <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger d-none" id="notif-badge" style="font-size:0.65rem">0</span>
          </div>
          <div class="lang-switcher">
            <button class="lang-btn ${lang === 'en' ? 'active' : ''}" data-lang="en" aria-label="English">EN</button>
            <button class="lang-btn ${lang === 'mr' ? 'active' : ''}" data-lang="mr" aria-label="मराठी">मरा</button>
          </div>
          <div class="dropdown">
            <div class="user-menu" data-bs-toggle="dropdown" aria-expanded="false" role="button" tabindex="0">
              <div class="user-avatar">${initials}</div>
              <div class="user-info d-none d-md-block">
                <div class="user-name">${user?.fullName_En || 'User'}</div>
                <div class="user-role">${role}</div>
              </div>
              <i class="bi bi-chevron-down" style="font-size:0.7rem;color:var(--text-muted)"></i>
            </div>
            <ul class="dropdown-menu dropdown-menu-end">
              <li><a class="dropdown-item" href="#/profile"><i class="bi bi-person me-2"></i>Profile</a></li>
              <li><hr class="dropdown-divider"></li>
              <li><button class="dropdown-item text-danger" id="btn-logout"><i class="bi bi-box-arrow-right me-2"></i><span data-i18n="auth.logout">Logout</span></button></li>
            </ul>
          </div>
        </div>
      </header>

      <!-- Main content -->
      <main class="main-content" id="main-content" aria-busy="false"></main>
    </div>
  `;

  buildSidebarNav(role);
  bindLayoutEvents();
  translateDOM();
}

/** Build sidebar navigation based on user role */
function buildSidebarNav(role) {
  const nav = $('sidebar-nav');
  if (!nav) return;

  const items = [];

  // Dashboard — everyone
  items.push({ href: '#/dashboard', icon: 'bi-speedometer2', label: 'nav.dashboard' });

  // Proposals section
  items.push({ section: 'nav.proposals' });

  if (['Submitter', 'CityEngineer', 'ADO', 'ChiefAccountant', 'DeputyCommissioner', 'Commissioner', 'Lotus'].includes(role)) {
    items.push({ href: '#/proposals', icon: 'bi-file-earmark-text', label: 'nav.myProposals' });
  }

  if (['CityEngineer', 'ADO', 'ChiefAccountant', 'DeputyCommissioner', 'Commissioner'].includes(role)) {
    items.push({ href: '#/approvals', icon: 'bi-check2-square', label: 'nav.pendingApprovals' });
  }

  if (['Commissioner', 'Auditor', 'Lotus'].includes(role)) {
    items.push({ href: '#/proposals/all', icon: 'bi-collection', label: 'nav.allProposals' });
  }

  if (['Submitter', 'Lotus'].includes(role)) {
    items.push({ href: '#/proposals/new/wizard', icon: 'bi-plus-circle', label: 'nav.newProposal' });
  }

  // Audit — Commissioner & Auditor (in main app), Lotus (in Lotus panel)
  if (['Commissioner', 'Auditor'].includes(role)) {
    items.push({ section: 'nav.audit' });
    items.push({ href: '#/audit', icon: 'bi-journal-text', label: 'nav.audit' });
  }

  // Lotus section
  if (role === 'Lotus') {
    items.push({ section: 'nav.lotus' });
    items.push({ href: '#/lotus', icon: 'bi-gear-wide-connected', label: 'nav.lotus' });
    items.push({ href: '#/lotus/users', icon: 'bi-people', label: 'nav.users' });
    items.push({ href: '#/lotus/departments', icon: 'bi-building', label: 'nav.departments' });
    items.push({ href: '#/lotus/designations', icon: 'bi-award', label: 'nav.designations' });
    items.push({ href: '#/lotus/fund-types', icon: 'bi-wallet2', label: 'nav.fundTypes' });
    items.push({ href: '#/lotus/account-heads', icon: 'bi-calculator', label: 'nav.accountHeads' });
    items.push({ href: '#/lotus/wards', icon: 'bi-geo-alt', label: 'nav.wards' });
    items.push({ href: '#/lotus/procurement-methods', icon: 'bi-cart-check', label: 'nav.procurementMethods' });
    items.push({ href: '#/lotus/audit', icon: 'bi-journal-text', label: 'nav.audit' });
    items.push({ href: '#/lotus/settings', icon: 'bi-sliders', label: 'nav.settings' });
  }

  let html = '';
  for (const item of items) {
    if (item.section) {
      html += `<li class="sidebar-nav-section" data-i18n="${item.section}"></li>`;
    } else {
      html += `
        <li class="sidebar-nav-item">
          <a class="sidebar-nav-link" href="${item.href}">
            <i class="bi ${item.icon}"></i>
            <span data-i18n="${item.label}"></span>
          </a>
        </li>`;
    }
  }
  nav.innerHTML = html;
}

/** Bind layout events — sidebar toggle, logout, language switcher */
function bindLayoutEvents() {
  // Sidebar toggle
  $('sidebar-toggle')?.addEventListener('click', () => {
    $('sidebar')?.classList.toggle('open');
  });

  $('sidebar-overlay')?.addEventListener('click', () => {
    $('sidebar')?.classList.remove('open');
  });

  // Logout
  $('btn-logout')?.addEventListener('click', () => {
    logout();
  });

  // Language switcher
  document.querySelectorAll('.lang-btn').forEach(btn => {
    btn.addEventListener('click', async () => {
      const lang = btn.dataset.lang;
      await setLang(lang);
      document.querySelectorAll('.lang-btn').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
    });
  });

  // Notification bell — load unread count
  loadNotificationBadge();
  $('notification-bell-wrapper')?.addEventListener('click', () => navigate('/notifications'));
}

/** Load unread notification count and update badge */
async function loadNotificationBadge() {
  try {
    const { default: api } = await import('./api.js');
    const result = await api.get('/v1/notifications/unread-count');
    const badge = $('notif-badge');
    if (!badge) return;
    const count = result?.count ?? result ?? 0;
    if (count > 0) {
      badge.textContent = count > 99 ? '99+' : count;
      badge.classList.remove('d-none');
    } else {
      badge.classList.add('d-none');
    }
  } catch { /* silently ignore */ }
}

/** Set the page title in the header */
export function setPageTitle(titleKey) {
  const el = $('page-title');
  if (el) {
    el.setAttribute('data-i18n', titleKey);
    translateDOM(el.parentElement);
  }
}

/** Get the main content container */
export function getContentArea() {
  return $('main-content');
}

/** Show loading state in content area */
export function showLoading() {
  const area = getContentArea();
  if (area) {
    area.setAttribute('aria-busy', 'true');
    area.innerHTML = `
      <div class="loading-container">
        <div class="spinner" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
      </div>`;
  }
}

// ── Route handlers ─────────────────────────────────────────────────

async function loginPage() {
  // Hide app layout, show login
  const { renderLogin } = await import('./pages/login.js');
  appEl.innerHTML = '';
  renderLogin(appEl);
  translateDOM();
}

async function dashboardPage() {
  showAppLayout();
  setPageTitle('nav.dashboard');
  const area = getContentArea();
  const { renderDashboardV1 } = await import('./pages/dashboard-v1.js');
  renderDashboardV1(area);
  translateDOM();
}

async function genericPage(title, content) {
  showAppLayout();
  setPageTitle(title);
  const area = getContentArea();
  area.innerHTML = content;
  translateDOM();
}

// ── Register routes ────────────────────────────────────────────────

addRoute('/login', loginPage);
addRoute('/dashboard', dashboardPage);

addRoute('/profile', async () => {
  showAppLayout(); setPageTitle('nav.profile');
  const { renderProfile } = await import('./pages/profile/profile.js');
  renderProfile(getContentArea()); translateDOM();
});

// Proposal routes
addRoute('/proposals', async () => {
  showAppLayout(); setPageTitle('nav.myProposals');
  const { renderProposalList } = await import('./pages/proposals/list.js');
  renderProposalList(getContentArea(), 'my'); translateDOM();
});

addRoute('/proposals/new', async () => {
  showAppLayout(); setPageTitle('nav.newProposal');
  const { renderProposalForm } = await import('./pages/proposals/form.js');
  renderProposalForm(getContentArea()); translateDOM();
}, ['Submitter', 'Lotus']);

addRoute('/proposals/all', async () => {
  showAppLayout(); setPageTitle('nav.allProposals');
  const { renderProposalList } = await import('./pages/proposals/list.js');
  renderProposalList(getContentArea(), 'all'); translateDOM();
}, ['Commissioner', 'Auditor', 'Lotus']);

addRoute('/approvals', async () => {
  showAppLayout(); setPageTitle('nav.pendingApprovals');
  const { renderProposalList } = await import('./pages/proposals/list.js');
  renderProposalList(getContentArea(), 'pending'); translateDOM();
}, ['CityEngineer', 'ADO', 'ChiefAccountant', 'DeputyCommissioner', 'Commissioner']);

addRoute('/proposals/:id', async (params) => {
  showAppLayout(); setPageTitle('nav.myProposals');
  const { renderProposalDetail } = await import('./pages/proposals/detail.js');
  renderProposalDetail(getContentArea(), params.id); translateDOM();
});

addRoute('/proposals/:id/edit', async (params) => {
  showAppLayout(); setPageTitle('nav.newProposal');
  const { renderWizard } = await import('./pages/proposals/wizard.js');
  renderWizard(getContentArea(), params.id); translateDOM();
}, ['Submitter', 'Lotus']);

addRoute('/audit', async () => {
  showAppLayout(); setPageTitle('nav.audit');
  const { renderAuditTrail } = await import('./pages/audit/audit.js');
  renderAuditTrail(getContentArea()); translateDOM();
}, ['Commissioner', 'Auditor']);

// Lotus routes
addRoute('/lotus', async () => {
  showAppLayout(); setPageTitle('nav.lotus');
  const { renderLotusDashboard } = await import('./pages/lotus/dashboard.js');
  renderLotusDashboard(getContentArea()); translateDOM();
}, ['Lotus']);

addRoute('/lotus/users', async () => {
  showAppLayout(); setPageTitle('nav.users');
  const { renderUsers } = await import('./pages/lotus/users.js');
  renderUsers(getContentArea()); translateDOM();
}, ['Lotus']);

addRoute('/lotus/departments', async () => {
  showAppLayout(); setPageTitle('nav.departments');
  const { renderDepartments } = await import('./pages/lotus/departments.js');
  renderDepartments(getContentArea()); translateDOM();
}, ['Lotus']);

addRoute('/lotus/designations', async () => {
  showAppLayout(); setPageTitle('nav.designations');
  const { renderDesignations } = await import('./pages/lotus/designations.js');
  renderDesignations(getContentArea()); translateDOM();
}, ['Lotus']);

addRoute('/lotus/fund-types', async () => {
  showAppLayout(); setPageTitle('nav.fundTypes');
  const { renderFundTypes } = await import('./pages/lotus/fund-types.js');
  renderFundTypes(getContentArea()); translateDOM();
}, ['Lotus']);

addRoute('/lotus/account-heads', async () => {
  showAppLayout(); setPageTitle('nav.accountHeads');
  const { renderAccountHeads } = await import('./pages/lotus/account-heads.js');
  renderAccountHeads(getContentArea()); translateDOM();
}, ['Lotus']);

addRoute('/lotus/wards', async () => {
  showAppLayout(); setPageTitle('nav.wards');
  const { renderWards } = await import('./pages/lotus/wards.js');
  renderWards(getContentArea()); translateDOM();
}, ['Lotus']);

addRoute('/lotus/procurement-methods', async () => {
  showAppLayout(); setPageTitle('nav.procurementMethods');
  const { renderProcurementMethods } = await import('./pages/lotus/procurement-methods.js');
  renderProcurementMethods(getContentArea()); translateDOM();
}, ['Lotus']);

addRoute('/lotus/audit', async () => {
  showAppLayout(); setPageTitle('nav.audit');
  const { renderAuditTrail } = await import('./pages/audit/audit.js');
  renderAuditTrail(getContentArea()); translateDOM();
}, ['Lotus']);
addRoute('/lotus/settings', () => genericPage('nav.settings', '<div class="card p-4"><p>Corporation Settings — coming in Phase 2</p></div>'), ['Lotus']);

// ── V1 Routes — Wizard & Notifications ─────────────────────────────

addRoute('/proposals/new/wizard', async () => {
  showAppLayout(); setPageTitle('nav.newProposal');
  const { renderWizard } = await import('./pages/proposals/wizard.js');
  const content = getContentArea();
  await renderWizard(content);
  translateDOM(content || undefined);
}, ['Submitter', 'Lotus']);

addRoute('/proposals/:id/wizard', async (params) => {
  showAppLayout(); setPageTitle('nav.newProposal');
  const { renderWizard } = await import('./pages/proposals/wizard.js');
  const content = getContentArea();
  await renderWizard(content, params.id);
  translateDOM(content || undefined);
}, ['Submitter', 'Lotus']);

addRoute('/notifications', async () => {
  showAppLayout(); setPageTitle('nav.notifications');
  const { renderNotifications } = await import('./pages/notifications.js');
  renderNotifications(getContentArea()); translateDOM();
});

// Approval console v1 — officer Note & Sign
addRoute('/proposals/:id/approve', async (params) => {
  showAppLayout(); setPageTitle('nav.pendingApprovals');
  const { renderApprovalConsole } = await import('./pages/proposals/approval-v1.js');
  renderApprovalConsole(getContentArea(), params.id); translateDOM();
}, ['CityEngineer', 'ADO', 'ChiefAccountant', 'DeputyCommissioner', 'Commissioner', 'Lotus']);

// ── Bootstrap ──────────────────────────────────────────────────────

async function boot() {
  await initI18n();
  initRouter();
}

boot().catch(err => console.error('App boot failed:', err));
