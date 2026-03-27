// Layout module — renders header and sidebar
import { getUser, logout, hasRole } from './auth.js';
import { api } from './api.js';
import { escapeHtml } from './utils.js';
import { t, getLang, createLangSelector, translatePage, onLangChange } from './i18n.js';

export function renderLayout() {
    const user = getUser();
    if (!user) return;

    renderHeader(user);
    renderSidebar(user);
}

export function clearLayout() {
    document.getElementById('app-header').innerHTML = '';
    document.getElementById('app-sidebar').innerHTML = '';
    document.getElementById('app-sidebar').className = '';
}

function renderHeader(user) {
    const header = document.getElementById('app-header');
    header.innerHTML = `
        <div class="app-header">
            <button class="btn btn-link text-dark d-md-none me-2 sidebar-toggle" aria-label="Toggle sidebar">
                <i class="bi bi-list fs-4"></i>
            </button>
            <div class="corp-title me-auto">
                <div>धुळे महानगरपालिका</div>
                <small>Dhule Municipal Corporation — Proposal Management</small>
            </div>
            <div class="d-flex align-items-center gap-3">
                <div id="lang-selector-slot"></div>
                <div class="dropdown" id="notif-dropdown">
                    <button class="btn btn-link text-dark position-relative p-0" data-bs-toggle="dropdown" aria-expanded="false" aria-label="Notifications">
                        <i class="bi bi-bell fs-5"></i>
                        <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger d-none" id="notif-badge" style="font-size:0.6rem"></span>
                    </button>
                    <ul class="dropdown-menu dropdown-menu-end shadow" style="min-width:320px;max-height:400px;overflow-y:auto;" id="notif-list">
                        <li class="dropdown-header">Notifications</li>
                        <li><div class="text-center py-2"><small class="text-muted">Loading...</small></div></li>
                    </ul>
                </div>
                <span class="d-none d-sm-inline text-muted" style="font-size:0.8rem;">
                    <i class="bi bi-person-circle me-1"></i>${user.fullName}
                    <span class="badge bg-secondary ms-1">${user.role}</span>
                </span>
                <button class="btn btn-outline-danger btn-sm" id="btn-logout" aria-label="Logout">
                    <i class="bi bi-box-arrow-right"></i>
                    <span class="d-none d-sm-inline ms-1" data-i18n="nav.logout">Logout</span>
                </button>
            </div>
        </div>`;

    // Insert language selector
    const slot = header.querySelector('#lang-selector-slot');
    if (slot) slot.appendChild(createLangSelector());

    document.getElementById('btn-logout').addEventListener('click', () => {
        logout();
        clearLayout();
    });

    // Mobile sidebar toggle
    const toggle = document.querySelector('.sidebar-toggle');
    if (toggle) {
        toggle.addEventListener('click', () => {
            document.getElementById('app-sidebar').classList.toggle('show');
        });
    }

    // Notifications
    loadNotifCount();
    const notifDrop = document.getElementById('notif-dropdown');
    notifDrop?.addEventListener('show.bs.dropdown', loadNotifList);
}

function renderSidebar(user) {
    const role = user.role;
    const nav = document.getElementById('app-sidebar');
    nav.className = 'app-sidebar';

    let links = `
        <div class="pt-2">
            <a href="#/dashboard" class="nav-link" data-route="/dashboard">
                <i class="bi bi-speedometer2"></i><span data-i18n="nav.dashboard">Dashboard</span>
            </a>`;

    // JE-specific
    if (role === 'JE' || role === 'Lotus') {
        links += `
            <a href="#/proposals/new" class="nav-link" data-route="/proposals/new">
                <i class="bi bi-plus-circle"></i><span data-i18n="nav.createProposal">Create Proposal</span>
            </a>
            <a href="#/proposals/my" class="nav-link" data-route="/proposals/my">
                <i class="bi bi-file-earmark-text"></i><span data-i18n="nav.myProposals">My Proposals</span>
            </a>`;
    }

    // Tasks / Pending approvals
    if (['JE','TS','AE','SE','CityEngineer'].includes(role) || role === 'Lotus') {
        links += `
            <a href="#/proposals/pending" class="nav-link" data-route="/proposals/pending">
                <i class="bi bi-clock-history"></i><span data-i18n="nav.assignedTasks">Assigned Tasks</span>
            </a>`;
    }
    if (['CityEngineer','AccountOfficer','DyCommissioner','Commissioner','StandingCommittee','Collector'].includes(role) || role === 'Lotus') {
        links += `
            <a href="#/approvals" class="nav-link" data-route="/approvals">
                <i class="bi bi-check2-square"></i><span data-i18n="nav.pendingApprovals">Pending Approvals</span>
            </a>`;
    }

    // All proposals (Commissioner, Auditor, Lotus)
    if (['Commissioner','Auditor','Lotus'].includes(role)) {
        links += `
            <a href="#/proposals/all" class="nav-link" data-route="/proposals/all">
                <i class="bi bi-collection"></i><span data-i18n="nav.allProposals">All Proposals</span>
            </a>
            <a href="#/audit" class="nav-link" data-route="/audit">
                <i class="bi bi-journal-text"></i><span data-i18n="nav.auditTrail">Audit Trail</span>
            </a>`;
    }

    links += `</div>`;

    // Lotus admin section
    if (role === 'Lotus') {
        links += `
            <div class="sidebar-section-label" data-i18n="nav.admin">Administration</div>
            <a href="#/admin/users" class="nav-link" data-route="/admin/users">
                <i class="bi bi-people"></i><span data-i18n="nav.users">Users</span>
            </a>
            <a href="#/admin/masters" class="nav-link" data-route="/admin/masters">
                <i class="bi bi-database"></i><span data-i18n="nav.masters">Masters</span>
            </a>`;
    }

    nav.innerHTML = links;

    // Highlight active link
    highlightActiveLink();
    window.addEventListener('hashchange', highlightActiveLink);

    // Translate sidebar labels
    translatePage(nav);

    // Re-translate sidebar on language change
    onLangChange(() => translatePage(nav));
}

function highlightActiveLink() {
    const hash = window.location.hash.slice(1) || '/dashboard';
    document.querySelectorAll('.app-sidebar .nav-link').forEach(link => {
        const route = link.getAttribute('data-route');
        link.classList.toggle('active', hash === route || hash.startsWith(route + '/'));
    });
}

// ── Notifications ────────────────────────────────────────────
async function loadNotifCount() {
    try {
        const res = await api.get('/notifications/unread-count');
        const badge = document.getElementById('notif-badge');
        if (!badge) return;
        const count = res.success ? (res.data || 0) : 0;
        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.classList.remove('d-none');
        } else {
            badge.classList.add('d-none');
        }
    } catch { /* silent */ }
}

async function loadNotifList() {
    const list = document.getElementById('notif-list');
    if (!list) return;
    list.innerHTML = '<li class="dropdown-header d-flex justify-content-between">Notifications<button class="btn btn-link btn-sm p-0" id="btn-read-all">Mark all read</button></li><li><div class="text-center py-2"><div class="spinner-border spinner-border-sm text-primary"></div></div></li>';

    const res = await api.get('/notifications?pageSize=15');
    const items = res.success ? (res.data?.items || res.data || []) : [];

    let html = '<li class="dropdown-header d-flex justify-content-between">Notifications<button class="btn btn-link btn-sm p-0" id="btn-read-all">Mark all read</button></li>';
    if (items.length === 0) {
        html += '<li><div class="text-center py-3 text-muted small">No notifications</div></li>';
    } else {
        items.forEach(n => {
            html += `<li><a class="dropdown-item py-2 ${n.isRead ? '' : 'bg-light'} btn-notif-read" href="#" data-id="${n.id}" style="white-space:normal;">
                <div class="fw-medium small">${escapeHtml(n.title || n.message || '—')}</div>
                <div class="text-muted" style="font-size:0.75rem;">${escapeHtml(n.message || '').substring(0, 80)}</div>
            </a></li>`;
        });
    }
    list.innerHTML = html;

    document.getElementById('btn-read-all')?.addEventListener('click', async e => {
        e.stopPropagation();
        await api.post('/notifications/read-all');
        loadNotifCount();
        loadNotifList();
    });
    list.querySelectorAll('.btn-notif-read').forEach(a => {
        a.addEventListener('click', async e => {
            e.preventDefault();
            if (a.classList.contains('bg-light')) {
                await api.post(`/notifications/${a.dataset.id}/read`);
                a.classList.remove('bg-light');
                loadNotifCount();
            }
        });
    });
}
