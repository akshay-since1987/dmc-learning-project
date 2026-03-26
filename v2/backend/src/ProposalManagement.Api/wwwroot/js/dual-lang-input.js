/**
 * Dual-language input component.
 * Renders paired input fields (English + Marathi) with bidirectional auto-translate.
 * Typing in either field auto-populates the other after a debounce delay.
 */

import { api } from './api.js';
import { debounce, escapeHtml } from './utils.js';

const TRANSLATE_DEBOUNCE_MS = 600;

function escapeAttr(str) {
    return (str || '').replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

/**
 * Create a dual-language input pair.
 * @param {object} opts
 * @param {string} opts.name       - Base field name (e.g. 'workTitle')
 * @param {string} opts.label      - Display label
 * @param {string} opts.type       - 'text' | 'textarea'
 * @param {boolean} opts.required  - Whether EN field is required
 * @param {string}  opts.valueEn   - Initial English value
 * @param {string}  opts.valueMr   - Initial Marathi value
 * @param {string}  opts.placeholderEn
 * @param {string}  opts.placeholderMr
 * @param {number}  opts.maxLength
 * @param {number}  opts.rows      - Textarea rows (default 3)
 * @returns {HTMLElement} wrapper element with .getValues() and .setValues() methods
 */
export function createDualLangInput(opts) {
    const wrapper = document.createElement('div');
    wrapper.className = 'dual-lang-group mb-3';

    const isTextarea = opts.type === 'textarea';
    const req = opts.required ? 'required' : '';
    const ml = opts.maxLength ? `maxlength="${opts.maxLength}"` : '';
    const rows = opts.rows || 3;

    wrapper.innerHTML = `
        <label class="form-label">${escapeHtml(opts.label)} ${opts.required ? '<span class="text-danger">*</span>' : ''}</label>
        <div class="row g-2">
            <div class="col-md-6">
                <div class="input-group input-group-sm mb-1">
                    <span class="input-group-text" style="font-size:0.75rem">EN</span>
                    ${isTextarea
                        ? `<textarea class="form-control" id="dual-${opts.name}-en" rows="${rows}"
                            placeholder="${escapeAttr(opts.placeholderEn || '')}" ${req} ${ml}
                            aria-label="${escapeAttr(opts.label)} (English)">${escapeHtml(opts.valueEn || '')}</textarea>`
                        : `<input type="text" class="form-control" id="dual-${opts.name}-en"
                            value="${escapeAttr(opts.valueEn || '')}"
                            placeholder="${escapeAttr(opts.placeholderEn || '')}" ${req} ${ml}
                            aria-label="${escapeAttr(opts.label)} (English)">`
                    }
                </div>
                <div class="invalid-feedback">This field is required.</div>
            </div>
            <div class="col-md-6">
                <div class="input-group input-group-sm mb-1">
                    <span class="input-group-text" style="font-size:0.75rem">मरा</span>
                    ${isTextarea
                        ? `<textarea class="form-control" id="dual-${opts.name}-mr" rows="${rows}" lang="mr"
                            placeholder="${escapeAttr(opts.placeholderMr || '')}" ${ml}
                            aria-label="${escapeAttr(opts.label)} (Marathi)">${escapeHtml(opts.valueMr || '')}</textarea>`
                        : `<input type="text" class="form-control" id="dual-${opts.name}-mr" lang="mr"
                            value="${escapeAttr(opts.valueMr || '')}"
                            placeholder="${escapeAttr(opts.placeholderMr || '')}" ${ml}
                            aria-label="${escapeAttr(opts.label)} (Marathi)">`
                    }
                </div>
            </div>
        </div>
    `;

    const enInput = wrapper.querySelector(`#dual-${opts.name}-en`);
    const mrInput = wrapper.querySelector(`#dual-${opts.name}-mr`);

    // Track active field to prevent translation loops
    let activeField = null;
    enInput.addEventListener('focus', () => { activeField = 'en'; });
    mrInput.addEventListener('focus', () => { activeField = 'mr'; });

    async function autoTranslate(sourceEl, targetEl, sourceLang, targetLang) {
        const text = sourceEl.value?.trim();
        if (!text) { targetEl.value = ''; return; }

        targetEl.classList.add('translating');
        try {
            const resp = await api.post('/translation/translate', { text, sourceLang, targetLang });
            if (resp.success && resp.data?.translatedText && document.activeElement === sourceEl) {
                targetEl.value = resp.data.translatedText;
            }
        } catch {
            // Silent fail — user can type manually
        } finally {
            targetEl.classList.remove('translating');
        }
    }

    const translateEnToMr = debounce(() => {
        if (activeField === 'en') autoTranslate(enInput, mrInput, 'en', 'mr');
    }, TRANSLATE_DEBOUNCE_MS);

    const translateMrToEn = debounce(() => {
        if (activeField === 'mr') autoTranslate(mrInput, enInput, 'mr', 'en');
    }, TRANSLATE_DEBOUNCE_MS);

    enInput.addEventListener('input', translateEnToMr);
    mrInput.addEventListener('input', translateMrToEn);

    // Public API on the wrapper element
    wrapper.getValues = () => ({
        en: enInput.value?.trim() || '',
        mr: mrInput.value?.trim() || ''
    });

    wrapper.setValues = (en, mr) => {
        if (isTextarea) {
            enInput.textContent = en || '';
            mrInput.textContent = mr || '';
        } else {
            enInput.value = en || '';
            mrInput.value = mr || '';
        }
    };

    return wrapper;
}

/**
 * Render a bilingual read-only display pair.
 * @param {string} label - Field label
 * @param {string} enText - English value
 * @param {string} mrText - Marathi value
 * @param {string} colClass - Bootstrap col class (default 'col-12')
 * @returns {string} HTML string
 */
export function bilingualDisplay(label, enText, mrText, colClass = 'col-12') {
    return `<div class="${colClass}">
        <label class="form-label text-muted small mb-0">${escapeHtml(label)}</label>
        <div class="d-flex gap-3 align-items-start">
            <div class="flex-fill">
                <span class="badge bg-light text-dark me-1" style="font-size:0.65rem">EN</span>
                <span class="fw-medium">${escapeHtml(enText || '—')}</span>
            </div>
            ${mrText ? `<div class="flex-fill">
                <span class="badge bg-light text-dark me-1" style="font-size:0.65rem">मरा</span>
                <span lang="mr">${escapeHtml(mrText)}</span>
            </div>` : ''}
        </div>
    </div>`;
}
