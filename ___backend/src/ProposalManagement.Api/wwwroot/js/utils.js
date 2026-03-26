/** Utility helpers */

/** Format a date string for display */
export function formatDate(dateStr) {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return d.toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
}

/** Format a date with time */
export function formatDateTime(dateStr) {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return d.toLocaleString('en-IN', {
    day: '2-digit', month: 'short', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  });
}

/** Format currency in Indian style */
export function formatCurrency(amount) {
  if (amount == null) return '';
  return '₹ ' + Number(amount).toLocaleString('en-IN', { minimumFractionDigits: 2 });
}

/** Debounce a function */
export function debounce(fn, delay = 300) {
  let timer;
  return (...args) => {
    clearTimeout(timer);
    timer = setTimeout(() => fn(...args), delay);
  };
}

/** Safely set text content */
export function setText(el, text) {
  if (el) el.textContent = text ?? '';
}

/** Safely set value */
export function setVal(el, value) {
  if (el) el.value = value ?? '';
}

/** Get element by ID shortcut */
export function $(id) {
  return document.getElementById(id);
}

/** Query selector shortcut */
export function $$(selector, root = document) {
  return root.querySelector(selector);
}

/** Query selector all shortcut */
export function $$$(selector, root = document) {
  return [...root.querySelectorAll(selector)];
}

/** Create element with attributes */
export function createElement(tag, attrs = {}, children = []) {
  const el = document.createElement(tag);
  for (const [key, val] of Object.entries(attrs)) {
    if (key === 'className') el.className = val;
    else if (key === 'textContent') el.textContent = val;
    else if (key === 'innerHTML') { /* skip for security */ }
    else if (key.startsWith('on')) el.addEventListener(key.slice(2).toLowerCase(), val);
    else el.setAttribute(key, val);
  }
  for (const child of children) {
    if (typeof child === 'string') el.appendChild(document.createTextNode(child));
    else if (child) el.appendChild(child);
  }
  return el;
}

/** Get initials from a name */
export function getInitials(name) {
  if (!name) return '?';
  return name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2);
}

/** Capitalize first letter */
export function capitalize(str) {
  if (!str) return '';
  return str.charAt(0).toUpperCase() + str.slice(1);
}

/** Map proposal stage enum to readable label */
export function stageToBadgeClass(stage) {
  const map = {
    'Draft': 'badge-draft',
    'AtCityEngineer': 'badge-at-city-engineer',
    'AtChiefAccountant': 'badge-at-chief-accountant',
    'AtDeputyCommissioner': 'badge-at-deputy-commissioner',
    'AtCommissioner': 'badge-at-commissioner',
    'Approved': 'badge-approved',
    'PushedBack': 'badge-pushed-back'
  };
  return map[stage] || 'badge-draft';
}

export function stageToLabel(stage) {
  const map = {
    'Draft': 'Draft',
    'AtCityEngineer': 'At City Engineer',
    'AtChiefAccountant': 'At Chief Accountant',
    'AtDeputyCommissioner': 'At Deputy Commissioner',
    'AtCommissioner': 'At Commissioner',
    'Approved': 'Approved',
    'PushedBack': 'Pushed Back'
  };
  return map[stage] || stage;
}
