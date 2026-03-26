// Login page
import { sendOtp, login } from '../auth.js';
import { renderLayout } from '../layout.js';
import { toast } from '../toast.js';

export async function renderLoginPage() {
    // Hide layout for login
    document.getElementById('app-header').innerHTML = '';
    document.getElementById('app-sidebar').innerHTML = '';
    document.getElementById('app-sidebar').className = '';

    document.getElementById('page-content').innerHTML = `
        <div class="login-container" style="position:fixed;inset:0;z-index:1050;">
            <div class="login-card text-center">
                <div class="mb-3">
                    <i class="bi bi-building text-primary" style="font-size:3rem;"></i>
                </div>
                <h5 class="mb-1" lang="mr">धुळे महानगरपालिका</h5>
                <p class="text-muted mb-4" style="font-size:0.85rem;">Dhule Municipal Corporation<br>Proposal Management System</p>

                <form id="login-form" novalidate>
                    <div class="mb-3 text-start">
                        <label for="mobile" class="form-label">
                            <i class="bi bi-phone me-1"></i>Mobile Number
                        </label>
                        <div class="input-group">
                            <span class="input-group-text">+91</span>
                            <input type="tel" class="form-control" id="mobile" placeholder="10-digit mobile"
                                maxlength="10" pattern="[0-9]{10}" autocomplete="tel" required
                                aria-describedby="mobile-help">
                        </div>
                        <div id="mobile-help" class="form-text">Enter your registered mobile number</div>
                    </div>

                    <button type="button" class="btn btn-primary w-100 mb-3" id="btn-send-otp">
                        <i class="bi bi-send me-1"></i>Send OTP
                    </button>

                    <div id="otp-section" class="d-none">
                        <div class="mb-3 text-start">
                            <label for="otp" class="form-label">
                                <i class="bi bi-shield-lock me-1"></i>OTP Code
                            </label>
                            <input type="text" class="form-control text-center fs-4 letter-spacing-2" id="otp"
                                placeholder="_ _ _ _ _ _" maxlength="6" pattern="[0-9]{6}"
                                autocomplete="one-time-code" required>
                        </div>

                        <button type="submit" class="btn btn-success w-100">
                            <i class="bi bi-box-arrow-in-right me-1"></i>Verify & Login
                        </button>
                    </div>

                    <div id="login-error" class="alert alert-danger mt-3 d-none" role="alert"></div>
                </form>

                <hr class="my-4">
                <small class="text-muted">© 2026 Dhule Municipal Corporation</small>
            </div>
        </div>`;

    const mobileInput = document.getElementById('mobile');
    const otpInput = document.getElementById('otp');
    const otpSection = document.getElementById('otp-section');
    const errorDiv = document.getElementById('login-error');
    const btnSend = document.getElementById('btn-send-otp');

    function showError(msg) {
        errorDiv.textContent = msg;
        errorDiv.classList.remove('d-none');
    }
    function hideError() {
        errorDiv.classList.add('d-none');
    }

    btnSend.addEventListener('click', async () => {
        hideError();
        const mobile = mobileInput.value.trim();
        if (!/^[0-9]{10}$/.test(mobile)) {
            showError('Please enter a valid 10-digit mobile number');
            mobileInput.focus();
            return;
        }

        btnSend.disabled = true;
        btnSend.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Sending...';

        const res = await sendOtp(mobile);
        btnSend.disabled = false;
        btnSend.innerHTML = '<i class="bi bi-send me-1"></i>Send OTP';

        if (res.success) {
            otpSection.classList.remove('d-none');
            otpInput.focus();
            toast.success('OTP sent successfully');
        } else {
            showError(res.error || 'Failed to send OTP');
        }
    });

    document.getElementById('login-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        hideError();

        const mobile = mobileInput.value.trim();
        const otp = otpInput.value.trim();

        if (!/^[0-9]{6}$/.test(otp)) {
            showError('Please enter a valid 6-digit OTP');
            otpInput.focus();
            return;
        }

        const submitBtn = e.target.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Verifying...';

        const res = await login(mobile, otp);
        submitBtn.disabled = false;
        submitBtn.innerHTML = '<i class="bi bi-box-arrow-in-right me-1"></i>Verify & Login';

        if (res.success) {
            toast.success('Login successful!');
            renderLayout();
            window.location.hash = '#/dashboard';
        } else {
            showError(res.error || 'Login failed');
        }
    });

    // Auto-focus mobile input
    mobileInput.focus();
}
