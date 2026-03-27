/**
 * i18n module — loads EN + alternate language JSON, provides t() helper,
 * language selector, and auto-translates DOM nodes with data-i18n attributes.
 *
 * Usage:
 *   import { t, getLang, setLang, translatePage, onLangChange } from './i18n.js';
 *   const label = t('proposal.form.subject');   // returns label in current lang
 *   setLang('mr');                               // switches UI language
 */

const STORAGE_KEY = 'pm_ui_lang';
const LANGS = { en: 'English', mr: 'मराठी' };

let currentLang = localStorage.getItem(STORAGE_KEY) || 'en';
let strings = { en: {}, mr: {} };
let loaded = false;
const listeners = [];

/**
 * Load both language files. Call once at app init.
 */
export async function initI18n() {
    if (loaded) return;
    try {
        const [enRes, mrRes] = await Promise.all([
            fetch('/i18n/en.json').then(r => r.json()),
            fetch('/i18n/mr.json').then(r => r.json())
        ]);
        strings.en = enRes;
        strings.mr = mrRes;
        loaded = true;
    } catch (e) {
        console.warn('i18n: failed to load language files', e);
    }
}

/**
 * Get the current UI language code ('en' or 'mr').
 */
export function getLang() {
    return currentLang;
}

/**
 * Get the alternate language code.
 */
export function getAltLang() {
    return currentLang === 'en' ? 'mr' : 'en';
}

/**
 * Available languages map.
 */
export function getLanguages() {
    return LANGS;
}

/**
 * Translate a key. Returns the string in the current language,
 * falling back to the other language, then the raw key.
 * @param {string} key - dot-separated i18n key
 * @param {string} [lang] - override language
 * @returns {string}
 */
export function t(key, lang) {
    const l = lang || currentLang;
    return strings[l]?.[key] || strings['en']?.[key] || key;
}

/**
 * Get bilingual label: "English Label / मराठी लेबल"
 * @param {string} key
 * @returns {string}
 */
export function tBilingual(key) {
    const en = strings['en']?.[key] || key;
    const mr = strings['mr']?.[key];
    if (mr && mr !== en) return `${en} / ${mr}`;
    return en;
}

/**
 * Switch UI language and re-translate all DOM nodes.
 * @param {string} lang - 'en' or 'mr'
 */
export function setLang(lang) {
    if (!LANGS[lang]) return;
    currentLang = lang;
    localStorage.setItem(STORAGE_KEY, lang);
    document.documentElement.lang = lang;
    translatePage();
    listeners.forEach(fn => fn(lang));
}

/**
 * Register a callback for language changes.
 * @param {function} fn - receives new lang code
 * @returns {function} unsubscribe
 */
export function onLangChange(fn) {
    listeners.push(fn);
    return () => {
        const idx = listeners.indexOf(fn);
        if (idx >= 0) listeners.splice(idx, 1);
    };
}

/**
 * Scan the DOM for data-i18n attributes and apply translations.
 * data-i18n="key"                → sets textContent
 * data-i18n-placeholder="key"    → sets placeholder
 * data-i18n-label="key"          → sets aria-label
 * data-i18n-html="key"           → sets innerHTML (use sparingly)
 */
export function translatePage(root = document) {
    root.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        el.textContent = t(key);
    });
    root.querySelectorAll('[data-i18n-placeholder]').forEach(el => {
        el.placeholder = t(el.getAttribute('data-i18n-placeholder'));
    });
    root.querySelectorAll('[data-i18n-label]').forEach(el => {
        el.setAttribute('aria-label', t(el.getAttribute('data-i18n-label')));
    });
    root.querySelectorAll('[data-i18n-html]').forEach(el => {
        el.innerHTML = t(el.getAttribute('data-i18n-html'));
    });
}

/**
 * Build a language selector dropdown (Bootstrap 5).
 * Returns an HTMLElement ready to insert in the header.
 */
export function createLangSelector() {
    const wrapper = document.createElement('div');
    wrapper.className = 'dropdown';
    wrapper.innerHTML = `
        <button class="btn btn-outline-secondary btn-sm dropdown-toggle" type="button"
                data-bs-toggle="dropdown" aria-expanded="false" id="lang-selector-btn">
            <i class="bi bi-translate me-1"></i><span id="lang-selector-label">${LANGS[currentLang]}</span>
        </button>
        <ul class="dropdown-menu dropdown-menu-end">
            ${Object.entries(LANGS).map(([code, label]) => `
                <li><button class="dropdown-item lang-option ${code === currentLang ? 'active' : ''}"
                    data-lang="${code}">${label}</button></li>
            `).join('')}
        </ul>
    `;
    wrapper.querySelectorAll('.lang-option').forEach(btn => {
        btn.addEventListener('click', () => {
            setLang(btn.dataset.lang);
            // Update selector UI
            wrapper.querySelectorAll('.lang-option').forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            wrapper.querySelector('#lang-selector-label').textContent = LANGS[btn.dataset.lang];
        });
    });
    return wrapper;
}
