/**
 * Dual-language input component.
 * Renders paired input fields (English + Marathi) with bidirectional auto-translate.
 * The user types in the primary language (based on i18n selection); the alternate
 * language field is readonly and auto-populated via translation API.
 * Labels always show both languages: "English Label / मराठी लेबल".
 */

import { api } from './api.js';
import { debounce, escapeHtml } from './utils.js';
import { getLang, getAltLang, onLangChange, t } from './i18n.js';

const TRANSLATE_DEBOUNCE_MS = 600;
const LANG_LABELS = { en: 'EN', mr: 'मराठी' };

function escapeAttr(str) {
    return (str || '').replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

/**
 * Create a dual-language input pair.
 * @param {object} opts
 * @param {string} opts.name        - Base field name (e.g. 'workTitle')
 * @param {string} opts.label       - Display label (English)
 * @param {string} opts.labelMr     - Display label (Marathi) — optional, looked up from i18n if not provided
 * @param {string} opts.i18nKey     - i18n key for label lookup (e.g. 'proposal.form.workTitle')
 * @param {string} opts.type        - 'text' | 'textarea'
 * @param {boolean} opts.required   - Whether primary field is required
 * @param {string}  opts.valueEn    - Initial English value
 * @param {string}  opts.valueMr    - Initial Marathi value
 * @param {string}  opts.placeholderEn
 * @param {string}  opts.placeholderMr
 * @param {number}  opts.maxLength
 * @param {number}  opts.rows       - Textarea rows (default 3)
 * @returns {HTMLElement} wrapper element with .getValues() and .setValues() methods
 */
export function createDualLangInput(opts) {
    const wrapper = document.createElement('div');
    wrapper.className = 'dual-lang-group mb-3';

    const isTextarea = opts.type === 'textarea';
    const req = opts.required ? 'required' : '';
    const ml = opts.maxLength ? `maxlength="${opts.maxLength}"` : '';
    const rows = opts.rows || 3;

    // Bilingual label: "English / मराठी"
    const labelEn = opts.label || '';
    const labelMr = opts.labelMr || (opts.i18nKey ? t(opts.i18nKey, 'mr') : '');
    const bilingualLabel = labelMr && labelMr !== labelEn
        ? `${escapeHtml(labelEn)} / <span lang="mr">${escapeHtml(labelMr)}</span>`
        : escapeHtml(labelEn);

    function buildFields() {
        const lang = getLang();
        const altLang = getAltLang();
        const primaryBadge = LANG_LABELS[lang] || lang.toUpperCase();
        const altBadge = LANG_LABELS[altLang] || altLang.toUpperCase();
        const primaryVal = lang === 'en' ? (opts._currentEn || opts.valueEn || '') : (opts._currentMr || opts.valueMr || '');
        const altVal = altLang === 'en' ? (opts._currentEn || opts.valueEn || '') : (opts._currentMr || opts.valueMr || '');
        const primaryPh = lang === 'en' ? (opts.placeholderEn || '') : (opts.placeholderMr || '');
        const altPh = altLang === 'en' ? (opts.placeholderEn || '') : (opts.placeholderMr || '');
        const primaryLangAttr = lang === 'mr' ? ' lang="mr"' : '';
        const altLangAttr = altLang === 'mr' ? ' lang="mr"' : '';

        wrapper.innerHTML = `
            <label class="form-label">${bilingualLabel} ${opts.required ? '<span class="text-danger">*</span>' : ''}</label>
            <div class="row g-2">
                <div class="col-md-6">
                    <div class="input-group input-group-sm mb-1">
                        <span class="input-group-text dual-lang-badge-primary" style="font-size:0.75rem">${primaryBadge}</span>
                        ${isTextarea
                            ? `<textarea class="form-control" id="dual-${opts.name}-primary" rows="${rows}"
                                placeholder="${escapeAttr(primaryPh)}" ${req} ${ml}${primaryLangAttr}
                                aria-label="${escapeAttr(labelEn)} (${primaryBadge})">${escapeHtml(primaryVal)}</textarea>`
                            : `<input type="text" class="form-control" id="dual-${opts.name}-primary"
                                value="${escapeAttr(primaryVal)}"
                                placeholder="${escapeAttr(primaryPh)}" ${req} ${ml}${primaryLangAttr}
                                aria-label="${escapeAttr(labelEn)} (${primaryBadge})">`
                        }
                    </div>
                    <div class="invalid-feedback">${t('common.required')}</div>
                </div>
                <div class="col-md-6">
                    <div class="input-group input-group-sm mb-1">
                        <span class="input-group-text dual-lang-badge-alt" style="font-size:0.75rem">${altBadge}</span>
                        ${isTextarea
                            ? `<textarea class="form-control bg-light" id="dual-${opts.name}-alt" rows="${rows}"
                                placeholder="${escapeAttr(altPh)}" ${ml} readonly tabindex="-1"${altLangAttr}
                                aria-label="${escapeAttr(labelEn)} (${altBadge})">${escapeHtml(altVal)}</textarea>`
                            : `<input type="text" class="form-control bg-light" id="dual-${opts.name}-alt"
                                value="${escapeAttr(altVal)}"
                                placeholder="${escapeAttr(altPh)}" ${ml} readonly tabindex="-1"${altLangAttr}
                                aria-label="${escapeAttr(labelEn)} (${altBadge})">`
                        }
                    </div>
                </div>
            </div>
        `;
    }

    buildFields();

    function getPrimaryInput() { return wrapper.querySelector(`#dual-${opts.name}-primary`); }
    function getAltInput() { return wrapper.querySelector(`#dual-${opts.name}-alt`); }

    // Auto-translate from primary → alt
    async function autoTranslate() {
        const primary = getPrimaryInput();
        const alt = getAltInput();
        if (!primary || !alt) return;

        const text = primary.value?.trim();
        if (!text) { alt.value = ''; return; }

        alt.classList.add('translating');
        try {
            const resp = await api.post('/translation/translate', {
                text, sourceLang: getLang(), targetLang: getAltLang()
            });
            if (resp.success && resp.data?.translatedText) {
                alt.value = resp.data.translatedText;
                // Cache both values
                if (getLang() === 'en') {
                    opts._currentEn = primary.value;
                    opts._currentMr = alt.value;
                } else {
                    opts._currentMr = primary.value;
                    opts._currentEn = alt.value;
                }
            }
        } catch {
            // Silent fail — user can see the primary value
        } finally {
            alt.classList.remove('translating');
        }
    }

    const debouncedTranslate = debounce(autoTranslate, TRANSLATE_DEBOUNCE_MS);

    // Attach input handler
    function attachListeners() {
        const primary = getPrimaryInput();
        if (primary) {
            primary.addEventListener('input', () => {
                // Update cached value
                if (getLang() === 'en') opts._currentEn = primary.value;
                else opts._currentMr = primary.value;
                debouncedTranslate();
            });
        }
    }
    attachListeners();

    // On language change, swap fields
    onLangChange(() => {
        // Save current values before rebuild
        const primary = getPrimaryInput();
        const alt = getAltInput();
        if (primary && alt) {
            if (getAltLang() === 'en') {
                // We just switched TO mr, so old primary was EN
                opts._currentEn = primary.value;
                opts._currentMr = alt.value;
            } else {
                opts._currentMr = primary.value;
                opts._currentEn = alt.value;
            }
        }
        buildFields();
        attachListeners();
    });

    // Public API
    wrapper.getValues = () => {
        const primary = getPrimaryInput();
        const alt = getAltInput();
        const pVal = primary?.value?.trim() || '';
        const aVal = alt?.value?.trim() || '';
        if (getLang() === 'en') return { en: pVal, mr: aVal };
        return { en: aVal, mr: pVal };
    };

    wrapper.setValues = (en, mr) => {
        opts._currentEn = en || '';
        opts._currentMr = mr || '';
        buildFields();
        attachListeners();
    };

    return wrapper;
}

/**
 * Render a bilingual read-only display pair.
 * Shows primary language prominently, alternate language below.
 * Both always visible regardless of language selection.
 * @param {string} label - Field label (English)
 * @param {string} enText - English value
 * @param {string} mrText - Marathi value
 * @param {string} colClass - Bootstrap col class (default 'col-12')
 * @param {string} labelMr - Marathi label (optional)
 * @returns {string} HTML string
 */
export function bilingualDisplay(label, enText, mrText, colClass = 'col-12', labelMr) {
    const biLabel = labelMr && labelMr !== label
        ? `${escapeHtml(label)} / <span lang="mr">${escapeHtml(labelMr)}</span>`
        : escapeHtml(label);
    return `<div class="${colClass}">
        <label class="form-label text-muted small mb-0">${biLabel}</label>
        <div class="d-flex gap-3 align-items-start">
            <div class="flex-fill">
                <span class="badge bg-light text-dark me-1" style="font-size:0.65rem">EN</span>
                <span class="fw-medium">${escapeHtml(enText || '—')}</span>
            </div>
            ${mrText ? `<div class="flex-fill">
                <span class="badge bg-light text-dark me-1" style="font-size:0.65rem">मराठी</span>
                <span lang="mr">${escapeHtml(mrText)}</span>
            </div>` : ''}
        </div>
    </div>`;
}
