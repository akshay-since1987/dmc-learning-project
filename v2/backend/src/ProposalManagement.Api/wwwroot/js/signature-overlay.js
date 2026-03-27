/**
 * Signature Overlay Component
 * Renders a PDF using pdf.js (NO ZOOM) and lets user drag / resize / rotate a signature image.
 * On confirm, returns placement metadata { pageNumber, x, y, width, height, rotation }.
 *
 * pdf.js is loaded from CDN (Mozilla).
 */

const PDFJS_CDN = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/4.9.155/pdf.min.mjs';
const PDFJS_WORKER_CDN = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/4.9.155/pdf.worker.min.mjs';

let pdfjsLib = null;

async function loadPdfJs() {
  if (pdfjsLib) return pdfjsLib;
  pdfjsLib = await import(PDFJS_CDN);
  pdfjsLib.GlobalWorkerOptions.workerSrc = PDFJS_WORKER_CDN;
  return pdfjsLib;
}

/**
 * @param {HTMLElement} container - DOM element to render into
 * @param {string} pdfUrl - URL/path to fetch the PDF
 * @param {string} signatureImageUrl - URL/path to the officer's signature image
 * @param {(meta: {pageNumber:number,x:number,y:number,width:number,height:number,rotation:number}) => void} onConfirm
 * @param {() => void} onCancel
 */
export async function renderSignatureOverlay(container, pdfUrl, signatureImageUrl, onConfirm, onCancel) {
  const lib = await loadPdfJs();
  const accessToken = localStorage.getItem('accessToken');
  const pdf = await lib.getDocument({
    url: pdfUrl,
    httpHeaders: accessToken ? { Authorization: `Bearer ${accessToken}` } : {}
  }).promise;

  const totalPages = pdf.numPages;
  let currentPage = 1;

  // State for signature drag/resize/rotate
  const sig = { x: 60, y: 60, width: 150, height: 60, rotation: 0 };
  let dragging = false, resizing = false;
  let dragStart = { mx: 0, my: 0, sx: 0, sy: 0 };

  container.innerHTML = `
    <div class="sig-overlay-wrapper">
      <div class="sig-toolbar d-flex align-items-center gap-2 p-2 border-bottom bg-light">
        <button class="btn btn-sm btn-outline-secondary" id="sig-prev" aria-label="Previous page"><i class="bi bi-chevron-left"></i></button>
        <span class="small" id="sig-page-info">Page 1 / ${totalPages}</span>
        <button class="btn btn-sm btn-outline-secondary" id="sig-next" aria-label="Next page"><i class="bi bi-chevron-right"></i></button>
        <div class="vr mx-1"></div>
        <label class="small mb-0">Rotate:</label>
        <input type="range" id="sig-rotate" min="-180" max="180" value="0" style="width:100px" aria-label="Rotate signature">
        <span class="small" id="sig-rot-val">0&deg;</span>
        <div class="ms-auto d-flex gap-1">
          <button class="btn btn-sm btn-success" id="sig-confirm"><i class="bi bi-check-lg me-1"></i>Sign Document</button>
          <button class="btn btn-sm btn-outline-secondary" id="sig-cancel">Cancel</button>
        </div>
      </div>
      <div class="sig-viewport-container" id="sig-viewport" style="position:relative;overflow:auto;background:#e9ecef;max-height:75vh">
        <canvas id="sig-canvas"></canvas>
        <div id="sig-handle" style="position:absolute;cursor:move;border:2px dashed var(--bs-primary);border-radius:4px;touch-action:none">
          <img id="sig-img" src="${signatureImageUrl}" alt="Signature" style="width:100%;height:100%;pointer-events:none;opacity:0.85" draggable="false">
          <div id="sig-resize" style="position:absolute;bottom:-4px;right:-4px;width:12px;height:12px;background:var(--bs-primary);border-radius:50%;cursor:nwse-resize"></div>
        </div>
      </div>
    </div>`;

  const canvas = container.querySelector('#sig-canvas');
  const viewport = container.querySelector('#sig-viewport');
  const handle = container.querySelector('#sig-handle');
  const resizeKnob = container.querySelector('#sig-resize');
  const rotateSlider = container.querySelector('#sig-rotate');
  const rotateVal = container.querySelector('#sig-rot-val');
  const pageInfo = container.querySelector('#sig-page-info');

  // ── Render PDF page ──
  async function renderPage(num) {
    const page = await pdf.getPage(num);
    // Fixed scale = 1.5 (NO user zoom for coordinate accuracy)
    const scale = 1.5;
    const vp = page.getViewport({ scale });
    canvas.width = vp.width;
    canvas.height = vp.height;
    const ctx = canvas.getContext('2d');
    await page.render({ canvasContext: ctx, viewport: vp }).promise;
    pageInfo.textContent = `Page ${num} / ${totalPages}`;
    updateHandlePosition();
  }

  function updateHandlePosition() {
    handle.style.left = sig.x + 'px';
    handle.style.top = sig.y + 'px';
    handle.style.width = sig.width + 'px';
    handle.style.height = sig.height + 'px';
    handle.style.transform = `rotate(${sig.rotation}deg)`;
  }

  updateHandlePosition();
  await renderPage(currentPage);

  // ── Drag ──
  handle.addEventListener('pointerdown', (e) => {
    if (e.target === resizeKnob) return;
    dragging = true;
    dragStart = { mx: e.clientX, my: e.clientY, sx: sig.x, sy: sig.y };
    handle.setPointerCapture(e.pointerId);
    e.preventDefault();
  });

  handle.addEventListener('pointermove', (e) => {
    if (!dragging && !resizing) return;
    if (dragging) {
      sig.x = Math.max(0, dragStart.sx + (e.clientX - dragStart.mx));
      sig.y = Math.max(0, dragStart.sy + (e.clientY - dragStart.my));
      updateHandlePosition();
    }
    if (resizing) {
      const dx = e.clientX - dragStart.mx;
      const dy = e.clientY - dragStart.my;
      sig.width = Math.max(40, dragStart.sx + dx);
      sig.height = Math.max(20, dragStart.sy + dy);
      updateHandlePosition();
    }
  });

  handle.addEventListener('pointerup', () => { dragging = false; });
  handle.addEventListener('lostpointercapture', () => { dragging = false; });

  // ── Resize ──
  resizeKnob.addEventListener('pointerdown', (e) => {
    resizing = true;
    dragStart = { mx: e.clientX, my: e.clientY, sx: sig.width, sy: sig.height };
    resizeKnob.setPointerCapture(e.pointerId);
    e.preventDefault();
    e.stopPropagation();
  });

  resizeKnob.addEventListener('pointermove', (e) => {
    if (!resizing) return;
    const dx = e.clientX - dragStart.mx;
    const dy = e.clientY - dragStart.my;
    sig.width = Math.max(40, dragStart.sx + dx);
    sig.height = Math.max(20, dragStart.sy + dy);
    updateHandlePosition();
  });

  resizeKnob.addEventListener('pointerup', () => { resizing = false; });
  resizeKnob.addEventListener('lostpointercapture', () => { resizing = false; });

  // ── Rotate slider ──
  rotateSlider.addEventListener('input', () => {
    sig.rotation = parseInt(rotateSlider.value, 10);
    rotateVal.textContent = sig.rotation + '\u00B0';
    updateHandlePosition();
  });

  // ── Page navigation ──
  container.querySelector('#sig-prev')?.addEventListener('click', async () => {
    if (currentPage > 1) { currentPage--; await renderPage(currentPage); }
  });
  container.querySelector('#sig-next')?.addEventListener('click', async () => {
    if (currentPage < totalPages) { currentPage++; await renderPage(currentPage); }
  });

  // ── Confirm / Cancel ──
  container.querySelector('#sig-confirm')?.addEventListener('click', () => {
    onConfirm({
      pageNumber: currentPage,
      x: Math.round(sig.x * 100) / 100,
      y: Math.round(sig.y * 100) / 100,
      width: Math.round(sig.width * 100) / 100,
      height: Math.round(sig.height * 100) / 100,
      rotation: sig.rotation
    });
  });

  container.querySelector('#sig-cancel')?.addEventListener('click', () => {
    if (onCancel) onCancel();
  });
}
