/** Toast notification module — aria-live for accessibility */

const TOAST_DURATION = 4000;
let container = null;

function ensureContainer() {
  if (container) return;
  container = document.createElement('div');
  container.className = 'toast-container';
  container.setAttribute('aria-live', 'polite');
  container.setAttribute('aria-atomic', 'true');
  document.body.appendChild(container);
}

/**
 * Show a toast notification.
 * @param {string} message - Text to display
 * @param {'success'|'danger'|'warning'|'info'} type - Bootstrap colour
 * @param {number} duration - MS to auto-dismiss (0 = manual close only)
 */
export function showToast(message, type = 'info', duration = TOAST_DURATION) {
  ensureContainer();

  const iconMap = {
    success: 'bi-check-circle-fill',
    danger: 'bi-exclamation-triangle-fill',
    warning: 'bi-exclamation-circle-fill',
    info: 'bi-info-circle-fill',
  };

  const toast = document.createElement('div');
  toast.className = `toast show align-items-center text-bg-${type} border-0`;
  toast.setAttribute('role', 'alert');
  toast.setAttribute('aria-live', 'assertive');
  toast.setAttribute('aria-atomic', 'true');

  const body = document.createElement('div');
  body.className = 'd-flex';

  const content = document.createElement('div');
  content.className = 'toast-body d-flex align-items-center gap-2';

  const icon = document.createElement('i');
  icon.className = `bi ${iconMap[type] || iconMap.info}`;

  const text = document.createElement('span');
  text.textContent = message;

  content.appendChild(icon);
  content.appendChild(text);

  const closeBtn = document.createElement('button');
  closeBtn.type = 'button';
  closeBtn.className = 'btn-close btn-close-white me-2 m-auto';
  closeBtn.setAttribute('aria-label', 'Close');
  closeBtn.addEventListener('click', () => removeToast(toast));

  body.appendChild(content);
  body.appendChild(closeBtn);
  toast.appendChild(body);

  container.appendChild(toast);

  if (duration > 0) {
    setTimeout(() => removeToast(toast), duration);
  }
}

function removeToast(toast) {
  toast.classList.add('hiding');
  toast.style.transition = 'opacity 300ms ease, transform 300ms ease';
  toast.style.opacity = '0';
  toast.style.transform = 'translateX(100%)';
  setTimeout(() => toast.remove(), 300);
}

export default { showToast };
