/** Bootstrap modal wrapper with focus-trap for a11y */

let activeModal = null;
let previousFocusEl = null;

/**
 * Show a modal dialog.
 * @param {object} opts
 * @param {string} opts.title - Modal title
 * @param {string|HTMLElement} opts.body - HTML string or DOM element for body
 * @param {string} opts.size - 'sm' | '' | 'lg' | 'xl'
 * @param {Array<{label:string,className:string,onClick:(close:Function)=>void}>} opts.buttons
 * @returns {{ close: Function, el: HTMLElement }}
 */
export function showModal(opts) {
  previousFocusEl = document.activeElement;
  closeActiveModal();

  const id = 'modal-' + Date.now();
  const sizeClass = opts.size ? `modal-${opts.size}` : '';

  const wrapper = document.createElement('div');
  wrapper.className = 'modal fade';
  wrapper.id = id;
  wrapper.tabIndex = -1;
  wrapper.setAttribute('role', 'dialog');
  wrapper.setAttribute('aria-labelledby', id + '-title');
  wrapper.setAttribute('aria-modal', 'true');

  const dialog = document.createElement('div');
  dialog.className = `modal-dialog ${sizeClass}`;
  dialog.setAttribute('role', 'document');

  const content = document.createElement('div');
  content.className = 'modal-content';

  // Header
  const header = document.createElement('div');
  header.className = 'modal-header';
  header.innerHTML = `
    <h5 class="modal-title" id="${id}-title">${escapeHtml(opts.title)}</h5>
    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
  `;

  // Body
  const body = document.createElement('div');
  body.className = 'modal-body';
  if (typeof opts.body === 'string') {
    body.innerHTML = opts.body;
  } else if (opts.body instanceof HTMLElement) {
    body.appendChild(opts.body);
  }

  // Footer
  const footer = document.createElement('div');
  footer.className = 'modal-footer';

  content.appendChild(header);
  content.appendChild(body);

  if (opts.buttons && opts.buttons.length > 0) {
    for (const btnConf of opts.buttons) {
      const btn = document.createElement('button');
      btn.type = 'button';
      btn.className = btnConf.className || 'btn btn-secondary';
      btn.textContent = btnConf.label;
      btn.addEventListener('click', () => {
        if (btnConf.onClick) btnConf.onClick(close);
      });
      footer.appendChild(btn);
    }
    content.appendChild(footer);
  }

  dialog.appendChild(content);
  wrapper.appendChild(dialog);
  document.body.appendChild(wrapper);

  /* global bootstrap */
  const bsModal = new bootstrap.Modal(wrapper, { backdrop: 'static', keyboard: true });
  activeModal = { bsModal, el: wrapper };

  wrapper.addEventListener('hidden.bs.modal', () => {
    wrapper.remove();
    activeModal = null;
    if (previousFocusEl) previousFocusEl.focus();
  });

  bsModal.show();

  function close() {
    bsModal.hide();
  }

  return { close, el: wrapper, body };
}

/**
 * Show a confirmation dialog.
 * @param {string} title
 * @param {string} message
 * @returns {Promise<boolean>}
 */
export function confirmModal(title, message) {
  return new Promise((resolve) => {
    const msgEl = document.createElement('p');
    msgEl.textContent = message;

    showModal({
      title,
      body: msgEl,
      buttons: [
        { label: 'Cancel', className: 'btn btn-secondary', onClick: (close) => { close(); resolve(false); } },
        { label: 'Confirm', className: 'btn btn-danger', onClick: (close) => { close(); resolve(true); } }
      ]
    });
  });
}

function closeActiveModal() {
  if (activeModal) {
    try { activeModal.bsModal.hide(); } catch { /* ignore */ }
    try { activeModal.el.remove(); } catch { /* ignore */ }
    activeModal = null;
  }
}

function escapeHtml(str) {
  const div = document.createElement('div');
  div.textContent = str;
  return div.innerHTML;
}
