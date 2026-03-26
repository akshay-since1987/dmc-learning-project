// Toast notification module
let toastCounter = 0;
const container = () => document.getElementById('toast-container');

function createToast(message, type = 'info', duration = 4000) {
    const id = `toast-${++toastCounter}`;
    const bgClass = {
        success: 'text-bg-success',
        danger: 'text-bg-danger',
        warning: 'text-bg-warning',
        info: 'text-bg-primary'
    }[type] || 'text-bg-primary';

    const icon = {
        success: 'bi-check-circle-fill',
        danger: 'bi-exclamation-triangle-fill',
        warning: 'bi-exclamation-circle-fill',
        info: 'bi-info-circle-fill'
    }[type] || 'bi-info-circle-fill';

    const html = `
        <div id="${id}" class="toast align-items-center ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi ${icon} me-2"></i>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>`;

    container().insertAdjacentHTML('beforeend', html);
    const el = document.getElementById(id);
    const toast = new bootstrap.Toast(el, { delay: duration });
    toast.show();
    el.addEventListener('hidden.bs.toast', () => el.remove());
}

export const toast = {
    success: (msg) => createToast(msg, 'success'),
    error: (msg) => createToast(msg, 'danger', 6000),
    warning: (msg) => createToast(msg, 'warning'),
    info: (msg) => createToast(msg, 'info')
};
