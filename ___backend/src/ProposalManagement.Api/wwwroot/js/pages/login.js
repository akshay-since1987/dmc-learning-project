/** Login page — OTP-based authentication */

import { sendOtp, verifyOtp } from '../auth.js';
import { showToast } from '../toast.js';
import { navigate } from '../router.js';

let currentStep = 'mobile';   // 'mobile' | 'otp'
let mobileNumber = '';
let isLotusUser = false;

export function renderLogin(container) {
  container.innerHTML = `
    <div class="login-wrapper">
      <!-- Left decorative panel (hidden < md) -->
      <div class="login-branding d-none d-md-flex">
        <div class="login-branding-content">
          <div class="login-branding-icon">
            <i class="bi bi-building"></i>
          </div>
          <h2>धुळे महानगरपालिका</h2>
          <p>Dhule Municipal Corporation</p>
          <div class="login-branding-divider"></div>
          <p class="login-branding-tagline">प्रशासकीय मान्यतेसाठी<br>कार्यालयीन टिपणी</p>
          <small>Administrative Approval Office Note</small>
        </div>
      </div>

      <!-- Right login form -->
      <div class="login-form-panel">
        <div class="login-card">
          <!-- Mobile header (shown < md only) -->
          <div class="login-header d-md-none">
            <div class="login-header-icon">
              <i class="bi bi-building"></i>
            </div>
            <h1>धुळे महानगरपालिका</h1>
            <p>Administrative Approval Office Note</p>
          </div>

          <div class="login-form-header">
            <h3>Welcome</h3>
            <p class="text-muted">Sign in to the Proposal Management System</p>
          </div>

          <!-- Step 1: Mobile Number -->
          <div id="step-mobile">
            <form id="form-mobile" novalidate>
              <div class="mb-4">
                <label for="inp-mobile" class="form-label fw-semibold" data-i18n="auth.mobileNumber">Mobile Number</label>
                <div class="input-group input-group-lg">
                  <span class="input-group-text bg-light border-end-0">
                    <i class="bi bi-phone me-1"></i>+91
                  </span>
                  <input type="tel" class="form-control border-start-0 ps-1" id="inp-mobile"
                    maxlength="10" pattern="[0-9]{10}" inputmode="numeric"
                    placeholder="Enter 10-digit mobile number"
                    aria-label="Mobile number" required autocomplete="tel">
                </div>
                <div class="invalid-feedback" id="mobile-error"></div>
              </div>
              <button type="submit" class="btn btn-primary btn-lg w-100 d-flex align-items-center justify-content-center gap-2" id="btn-send-otp">
                <i class="bi bi-send"></i>
                <span data-i18n="auth.sendOtp">Send OTP</span>
              </button>
            </form>
          </div>

          <!-- Step 2: OTP (+ optional password for Lotus) -->
          <div id="step-otp" class="d-none">
            <div class="otp-sent-banner">
              <div class="d-flex align-items-center gap-2 mb-2">
                <span class="otp-sent-icon"><i class="bi bi-check-circle-fill"></i></span>
                <span>OTP sent to <strong id="display-mobile"></strong></span>
              </div>
              <button class="btn btn-sm btn-outline-secondary" id="btn-change-number">
                <i class="bi bi-arrow-left me-1"></i><span data-i18n="auth.backToMobile">Change Number</span>
              </button>
            </div>

            <form id="form-otp" novalidate>
              <div class="mb-3">
                <label for="otp-0" class="form-label fw-semibold" data-i18n="auth.enterOtp">Enter OTP</label>
                <div class="otp-inputs" id="otp-input-group">
                  <input type="text" class="otp-box" maxlength="1" inputmode="numeric" pattern="[0-9]" id="otp-0" aria-label="Digit 1" autocomplete="off">
                  <input type="text" class="otp-box" maxlength="1" inputmode="numeric" pattern="[0-9]" id="otp-1" aria-label="Digit 2" autocomplete="off">
                  <input type="text" class="otp-box" maxlength="1" inputmode="numeric" pattern="[0-9]" id="otp-2" aria-label="Digit 3" autocomplete="off">
                  <input type="text" class="otp-box" maxlength="1" inputmode="numeric" pattern="[0-9]" id="otp-3" aria-label="Digit 4" autocomplete="off">
                  <input type="text" class="otp-box" maxlength="1" inputmode="numeric" pattern="[0-9]" id="otp-4" aria-label="Digit 5" autocomplete="off">
                  <input type="text" class="otp-box" maxlength="1" inputmode="numeric" pattern="[0-9]" id="otp-5" aria-label="Digit 6" autocomplete="off">
                </div>
                <div class="invalid-feedback d-block" id="otp-error"></div>
              </div>

              <!-- Password field (shown only for Lotus users) -->
              <div class="mb-3 d-none" id="password-group">
                <label for="inp-password" class="form-label fw-semibold" data-i18n="auth.password">Password</label>
                <div class="input-group input-group-lg">
                  <span class="input-group-text bg-light border-end-0">
                    <i class="bi bi-lock"></i>
                  </span>
                  <input type="password" class="form-control border-start-0 ps-1" id="inp-password"
                    placeholder="Enter your password" aria-label="Password" autocomplete="current-password">
                </div>
                <div class="invalid-feedback" id="password-error"></div>
              </div>

              <button type="submit" class="btn btn-primary btn-lg w-100 d-flex align-items-center justify-content-center gap-2" id="btn-verify-otp">
                <i class="bi bi-shield-lock"></i>
                <span data-i18n="auth.verifyOtp">Verify OTP</span>
              </button>

              <div class="text-center mt-3">
                <button type="button" class="btn btn-link btn-sm text-decoration-none" id="btn-resend-otp" disabled>
                  <i class="bi bi-arrow-clockwise me-1"></i><span data-i18n="auth.resendOtp">Resend OTP</span>
                </button>
                <span class="text-muted ms-1 small" id="resend-timer"></span>
              </div>
            </form>
          </div>

          <div class="login-footer">
            <small class="text-muted">© 2025 Dhule Municipal Corporation</small>
          </div>
        </div>
      </div>
    </div>
  `;

  // Wire up OTP box auto-advance
  setupOtpBoxes(container);

  currentStep = 'mobile';
  bindLoginEvents(container);
}

/** Wire up the 6 individual OTP input boxes for auto-advance / backspace */
function setupOtpBoxes(container) {
  const boxes = container.querySelectorAll('.otp-box');
  boxes.forEach((box, i) => {
    box.addEventListener('input', (e) => {
      const v = e.target.value.replace(/\D/g, '');
      e.target.value = v.slice(0, 1);
      if (v && i < boxes.length - 1) boxes[i + 1].focus();
    });
    box.addEventListener('keydown', (e) => {
      if (e.key === 'Backspace' && !box.value && i > 0) {
        boxes[i - 1].focus();
      }
    });
    box.addEventListener('paste', (e) => {
      e.preventDefault();
      const text = (e.clipboardData || window.clipboardData).getData('text').replace(/\D/g, '').slice(0, 6);
      for (let j = 0; j < text.length && j < boxes.length; j++) {
        boxes[j].value = text[j];
      }
      const next = Math.min(text.length, boxes.length - 1);
      boxes[next].focus();
    });
  });
}

/** Collect OTP string from the 6 boxes */
function getOtpValue(container) {
  return Array.from(container.querySelectorAll('.otp-box')).map(b => b.value).join('');
}

function bindLoginEvents(container) {
  const formMobile = container.querySelector('#form-mobile');
  const formOtp = container.querySelector('#form-otp');
  const btnChangeNumber = container.querySelector('#btn-change-number');
  const btnResendOtp = container.querySelector('#btn-resend-otp');

  // Step 1: Send OTP
  formMobile.addEventListener('submit', async (e) => {
    e.preventDefault();
    const inp = container.querySelector('#inp-mobile');
    const mobile = inp.value.trim();

    if (!/^[0-9]{10}$/.test(mobile)) {
      inp.classList.add('is-invalid');
      container.querySelector('#mobile-error').textContent = 'Please enter a valid 10-digit mobile number';
      return;
    }
    inp.classList.remove('is-invalid');

    const btn = container.querySelector('#btn-send-otp');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Sending...';

    try {
      await sendOtp(mobile);
      mobileNumber = mobile;
      showStep('otp', container);
      showToast('OTP sent successfully', 'success');
      startResendTimer(container);
    } catch (err) {
      showToast(err.message || 'Failed to send OTP', 'danger');
    } finally {
      btn.disabled = false;
      btn.innerHTML = '<i class="bi bi-send me-2"></i><span data-i18n="auth.sendOtp">Send OTP</span>';
    }
  });

  // Step 2: Verify OTP
  formOtp.addEventListener('submit', async (e) => {
    e.preventDefault();
    const otp = getOtpValue(container);

    if (!/^[0-9]{6}$/.test(otp)) {
      container.querySelector('#otp-error').textContent = 'Please enter the 6-digit OTP';
      container.querySelectorAll('.otp-box').forEach(b => b.classList.add('is-invalid'));
      return;
    }
    container.querySelector('#otp-error').textContent = '';
    container.querySelectorAll('.otp-box').forEach(b => b.classList.remove('is-invalid'));

    const password = container.querySelector('#inp-password')?.value || null;

    const btn = container.querySelector('#btn-verify-otp');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Verifying...';

    try {
      const data = await verifyOtp(mobileNumber, otp, password);
      isLotusUser = data.user?.role === 'Lotus';
      showToast('Login successful!', 'success');
      navigate('/dashboard');
    } catch (err) {
      showToast(err.message || 'Login failed', 'danger');
      // If error mentions password, show the password field
      if (err.message?.toLowerCase().includes('password')) {
        container.querySelector('#password-group')?.classList.remove('d-none');
        isLotusUser = true;
      }
    } finally {
      btn.disabled = false;
      btn.innerHTML = '<i class="bi bi-shield-lock me-2"></i><span data-i18n="auth.verifyOtp">Verify OTP</span>';
    }
  });

  // Back to mobile
  btnChangeNumber.addEventListener('click', () => {
    showStep('mobile', container);
  });

  // Resend OTP
  btnResendOtp.addEventListener('click', async () => {
    btnResendOtp.disabled = true;
    try {
      await sendOtp(mobileNumber);
      showToast('OTP resent', 'success');
      startResendTimer(container);
    } catch (err) {
      showToast(err.message || 'Failed to resend OTP', 'danger');
      btnResendOtp.disabled = false;
    }
  });
}

function showStep(step, container) {
  currentStep = step;
  const stepMobile = container.querySelector('#step-mobile');
  const stepOtp = container.querySelector('#step-otp');

  if (step === 'mobile') {
    stepMobile.classList.remove('d-none');
    stepOtp.classList.add('d-none');
    container.querySelector('#inp-mobile')?.focus();
  } else {
    stepMobile.classList.add('d-none');
    stepOtp.classList.remove('d-none');
    container.querySelector('#display-mobile').textContent = `+91 ${mobileNumber}`;
    container.querySelector('#otp-0')?.focus();
  }
}

let resendInterval = null;

function startResendTimer(container) {
  const btn = container.querySelector('#btn-resend-otp');
  const timerEl = container.querySelector('#resend-timer');
  btn.disabled = true;

  let seconds = 30;
  timerEl.textContent = `(${seconds}s)`;

  clearInterval(resendInterval);
  resendInterval = setInterval(() => {
    seconds--;
    if (seconds <= 0) {
      clearInterval(resendInterval);
      timerEl.textContent = '';
      btn.disabled = false;
    } else {
      timerEl.textContent = `(${seconds}s)`;
    }
  }, 1000);
}
