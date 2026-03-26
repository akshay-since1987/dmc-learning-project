/**
 * Dual-language input component.
 * Renders paired input fields (English + Alternate language) with bidirectional auto-translate.
 * Typing in either field auto-populates the other after a debounce delay.
 */

import api from './api.js';
import { debounce } from './utils.js';

const TRANSLATE_DEBOUNCE_MS = 600;

/**
 * Create a dual-language input pair and append it to a container.
 * @param {object} opts
 * @param {string} opts.name - Base field name (e.g. 'subject')
 * @param {string} opts.label - Display label
 * @param {string} opts.type - 'text' | 'textarea'
 * @param {boolean} opts.required
 * @param {string} opts.valueEn - Initial English value
 * @param {string} opts.valueAlt - Initial Alt value
 * @param {string} opts.placeholderEn
 * @param {string} opts.placeholderAlt
 * @param {number} opts.maxLength
 * @returns {HTMLElement} - The wrapper element with .getValues() method
 */
export function createDualLangInput(opts) {
  const wrapper = document.createElement('div');
  wrapper.className = 'dual-lang-group mb-3';

  const isTextarea = opts.type === 'textarea';
  const req = opts.required ? 'required' : '';
  const ml = opts.maxLength ? `maxlength="${opts.maxLength}"` : '';

  wrapper.innerHTML = `
    <label class="form-label">${escapeHtml(opts.label)} ${opts.required ? '<span class="text-danger">*</span>' : ''}</label>
    <div class="row g-2">
      <div class="col-md-6">
        <div class="input-group input-group-sm mb-1">
          <span class="input-group-text" style="font-size:0.75rem">EN</span>
          ${isTextarea
            ? `<textarea class="form-control" id="dual-${opts.name}-en" rows="3"
                placeholder="${opts.placeholderEn || ''}" ${req} ${ml}
                aria-label="${opts.label} (English)">${escapeHtml(opts.valueEn || '')}</textarea>`
            : `<input type="text" class="form-control" id="dual-${opts.name}-en"
                value="${escapeAttr(opts.valueEn || '')}"
                placeholder="${opts.placeholderEn || ''}" ${req} ${ml}
                aria-label="${opts.label} (English)">`
          }
        </div>
        <div class="invalid-feedback">This field is required.</div>
      </div>
      <div class="col-md-6">
        <div class="input-group input-group-sm mb-1">
          <span class="input-group-text" style="font-size:0.75rem">मरा</span>
          ${isTextarea
            ? `<textarea class="form-control" id="dual-${opts.name}-alt" rows="3" lang="mr"
                placeholder="${opts.placeholderAlt || ''}" ${req} ${ml}
                aria-label="${opts.label} (Marathi)">${escapeHtml(opts.valueAlt || '')}</textarea>`
            : `<input type="text" class="form-control" id="dual-${opts.name}-alt" lang="mr"
                value="${escapeAttr(opts.valueAlt || '')}"
                placeholder="${opts.placeholderAlt || ''}" ${req} ${ml}
                aria-label="${opts.label} (Marathi)">`
          }
        </div>
      </div>
    </div>
  `;

  const enInput = wrapper.querySelector(`#dual-${opts.name}-en`);
  const altInput = wrapper.querySelector(`#dual-${opts.name}-alt`);

  // Track which field the user is actively typing in to avoid translation loops
  let activeField = null;

  enInput.addEventListener('focus', () => { activeField = 'en'; });
  altInput.addEventListener('focus', () => { activeField = 'alt'; });

  /** Translate text and set the target input, with a subtle loading indicator */
  async function autoTranslate(sourceEl, targetEl, sourceLang, targetLang) {
    const text = sourceEl.value?.trim();
    if (!text) { targetEl.value = ''; return; }

    targetEl.classList.add('translating');
    try {
      const resp = await api.post('/translation/translate', { text, sourceLang, targetLang });
      // Only apply if the source field is still focused (user hasn't switched)
      if (resp.translatedText && document.activeElement === sourceEl) {
        targetEl.value = resp.translatedText;
      }
    } catch {
      // Silent fail — user can type manually
    } finally {
      targetEl.classList.remove('translating');
    }
  }

  // EN → Alt (debounced)
  const translateEnToAlt = debounce(() => {
    if (activeField === 'en') autoTranslate(enInput, altInput, 'en', 'mr');
  }, TRANSLATE_DEBOUNCE_MS);

  // Alt → EN (debounced)
  const translateAltToEn = debounce(() => {
    if (activeField === 'alt') autoTranslate(altInput, enInput, 'mr', 'en');
  }, TRANSLATE_DEBOUNCE_MS);

  enInput.addEventListener('input', translateEnToAlt);
  altInput.addEventListener('input', translateAltToEn);

  // Attach getValues method to the wrapper
  wrapper.getValues = () => ({
    en: enInput.value?.trim() || '',
    alt: altInput.value?.trim() || ''
  });

  wrapper.setValues = (en, alt) => {
    if (isTextarea) {
      enInput.textContent = en || '';
      altInput.textContent = alt || '';
    } else {
      enInput.value = en || '';
      altInput.value = alt || '';
    }
  };

  return wrapper;
}

function escapeHtml(str) {
  const div = document.createElement('div');
  div.textContent = str || '';
  return div.innerHTML;
}

function escapeAttr(str) {
  return (str || '').replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}
