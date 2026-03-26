/** Notifications page — lists in-app notifications with read/unread state */

import api from '../api.js';
import { formatDateTime } from '../utils.js';
import { translateDOM } from '../i18n.js';

/**
 * @param {HTMLElement} container
 */
export async function renderNotifications(container) {
  container.innerHTML = `
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h5 class="fw-semibold mb-0"><i class="bi bi-bell me-2"></i>Notifications</h5>
      <button class="btn btn-sm btn-outline-primary" id="mark-all-read">
        <i class="bi bi-check2-all me-1"></i>Mark all as read
      </button>
    </div>
    <div id="notif-list">
      <div class="text-center py-5"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></div>
    </div>
  `;

  const markAllBtn = container.querySelector('#mark-all-read');
  markAllBtn?.addEventListener('click', async () => {
    try {
      await api.post('/v1/notifications/read-all');
      loadNotifications(container);
      updateBellBadge();
    } catch { /* ignore */ }
  });

  loadNotifications(container);
}

async function loadNotifications(container) {
  const listEl = container.querySelector('#notif-list');
  try {
    const items = await api.get('/v1/notifications');
    if (!items?.length) {
      listEl.innerHTML = `
        <div class="text-center py-5 text-muted">
          <i class="bi bi-bell-slash fs-1 d-block mb-2"></i>
          <p>No notifications yet</p>
        </div>`;
      return;
    }

    let html = '<div class="list-group list-group-flush">';
    for (const n of items) {
      const unreadClass = n.isRead ? '' : 'bg-primary bg-opacity-10 border-start border-primary border-3';
      html += `
        <div class="list-group-item ${unreadClass} py-3" data-notif-id="${escapeAttr(n.id)}" style="cursor:pointer">
          <div class="d-flex justify-content-between align-items-start">
            <div class="flex-grow-1">
              <div class="fw-semibold ${n.isRead ? 'text-muted' : ''}">${escapeHtml(n.title_En)}</div>
              <div class="small mt-1">${escapeHtml(n.message_En)}</div>
              <small class="text-muted">${formatDateTime(n.createdAt)}</small>
            </div>
            ${!n.isRead ? '<span class="badge bg-primary rounded-pill ms-2">New</span>' : ''}
          </div>
        </div>`;
    }
    html += '</div>';
    listEl.innerHTML = html;

    // Click to mark read & navigate
    listEl.querySelectorAll('[data-notif-id]').forEach(el => {
      el.addEventListener('click', async () => {
        const id = el.dataset.notifId;
        const item = items.find(i => i.id === id);
        if (item && !item.isRead) {
          try {
            await api.post(`/v1/notifications/${id}/read`);
            el.classList.remove('bg-primary', 'bg-opacity-10', 'border-start', 'border-primary', 'border-3');
            el.querySelector('.badge')?.remove();
            updateBellBadge();
          } catch { /* ignore */ }
        }
        if (item?.linkUrl) {
          window.location.hash = item.linkUrl;
        }
      });
    });

    translateDOM(container);
  } catch (err) {
    listEl.innerHTML = `<div class="alert alert-danger">Failed to load notifications</div>`;
  }
}

async function updateBellBadge() {
  try {
    const result = await api.get('/v1/notifications/unread-count');
    const badge = document.getElementById('notif-badge');
    if (!badge) return;
    const count = result?.count ?? result ?? 0;
    if (count > 0) {
      badge.textContent = count > 99 ? '99+' : count;
      badge.classList.remove('d-none');
    } else {
      badge.classList.add('d-none');
    }
  } catch { /* ignore */ }
}

function escapeHtml(str) {
  const div = document.createElement('div');
  div.textContent = str || '';
  return div.innerHTML;
}

function escapeAttr(str) {
  return String(str || '').replace(/[&"'<>]/g, c => ({
    '&': '&amp;', '"': '&quot;', "'": '&#39;', '<': '&lt;', '>': '&gt;'
  })[c]);
}
