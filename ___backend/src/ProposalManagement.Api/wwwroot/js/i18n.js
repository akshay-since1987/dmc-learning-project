/** i18n module — loads en.json / {alt}.json, translates DOM nodes */

let currentLang = localStorage.getItem('lang') || 'en';
let strings = {};     // { en: {...}, mr: {...} }
let loadedLangs = new Set();

/** Load a language file */
async function loadLanguage(lang) {
  if (loadedLangs.has(lang)) return;
  try {
    const resp = await fetch(`/i18n/${lang}.json`);
    if (resp.ok) {
      strings[lang] = await resp.json();
      loadedLangs.add(lang);
    }
  } catch (e) {
    console.warn(`Failed to load i18n/${lang}.json`, e);
  }
}

/** Get translated string by key */
export function t(key) {
  return strings[currentLang]?.[key] ?? strings['en']?.[key] ?? key;
}

/** Get alt language string (always the non-current language, typically 'mr') */
export function tAlt(key) {
  const altLang = document.documentElement.dataset.altLang || 'mr';
  return strings[altLang]?.[key] ?? key;
}

/** Get current language */
export function getLang() {
  return currentLang;
}

/** Set language and re-translate DOM */
export async function setLang(lang) {
  await loadLanguage(lang);
  currentLang = lang;
  localStorage.setItem('lang', lang);
  translateDOM();
  document.documentElement.lang = lang;
  window.dispatchEvent(new CustomEvent('lang:changed', { detail: { lang } }));
}

/** Translate all DOM nodes with data-i18n attribute */
export function translateDOM(root = document) {
  const nodes = root.querySelectorAll('[data-i18n]');
  for (const node of nodes) {
    const key = node.getAttribute('data-i18n');
    const translated = t(key);
    // Handle placeholder vs textContent
    if (node.tagName === 'INPUT' || node.tagName === 'TEXTAREA') {
      node.placeholder = translated;
    } else {
      node.textContent = translated;
    }
  }
  // Translate aria-label
  const ariaNodes = root.querySelectorAll('[data-i18n-aria]');
  for (const node of ariaNodes) {
    node.setAttribute('aria-label', t(node.getAttribute('data-i18n-aria')));
  }
}

/** Initialize i18n — load both languages */
export async function initI18n() {
  await loadLanguage('en');
  const altLang = document.documentElement.dataset.altLang || 'mr';
  await loadLanguage(altLang);
  translateDOM();
}

export default { t, getLang, setLang, translateDOM, initI18n };
