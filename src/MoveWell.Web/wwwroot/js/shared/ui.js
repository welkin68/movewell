// MoveWell shared UI behaviour (progressive enhancement: every page works without this file).
// Loaded as an ES module; no inline scripts (CSP script-src 'self').

const ready = (fn) => (document.readyState === "loading" ? document.addEventListener("DOMContentLoaded", fn) : fn());

/** Show/hide password: <button data-toggle-password="inputId" aria-pressed="false"> */
function initPasswordToggles() {
  document.querySelectorAll("[data-toggle-password]").forEach((button) => {
    const input = document.getElementById(button.dataset.togglePassword);
    if (!input) return;
    button.hidden = false;
    button.addEventListener("click", () => {
      const show = input.type === "password";
      input.type = show ? "text" : "password";
      button.setAttribute("aria-pressed", String(show));
      button.querySelector("use")?.setAttribute("href", button.dataset[show ? "iconHide" : "iconShow"]);
    });
  });
}

/** OTP field: keep digits only (Persian/Arabic digits converted), auto-submit when complete. */
function initOtpInputs() {
  const toLatin = (value) =>
    value
      .replace(/[۰-۹]/g, (d) => String(d.charCodeAt(0) - 0x06f0))
      .replace(/[٠-٩]/g, (d) => String(d.charCodeAt(0) - 0x0660))
      .replace(/\D/g, "");

  document.querySelectorAll("[data-otp]").forEach((input) => {
    const length = Number(input.maxLength) || 6;
    input.addEventListener("input", () => {
      const clean = toLatin(input.value).slice(0, length);
      if (clean !== input.value) input.value = clean;
    });
  });
}

/** Resend countdown: <button data-countdown="120"> stays disabled until it reaches 0. */
function initCountdowns() {
  document.querySelectorAll("[data-countdown]").forEach((button) => {
    let left = Number(button.dataset.countdown) || 0;
    const label = button.querySelector("[data-countdown-label]");
    const original = label?.textContent ?? "";
    const tick = () => {
      if (left <= 0) {
        button.disabled = false;
        if (label) label.textContent = original;
        return;
      }
      button.disabled = true;
      if (label) label.textContent = `${original} (${left.toLocaleString(document.documentElement.lang)})`;
      left -= 1;
      setTimeout(tick, 1000);
    };
    tick();
  });
}

/** Flash toast: auto-hide after 6 s, close button. */
function initToasts() {
  document.querySelectorAll("[data-toast]").forEach((toast) => {
    const hide = () => (toast.hidden = true);
    toast.querySelector("[data-toast-close]")?.addEventListener("click", hide);
    setTimeout(hide, 6000);
  });
}

/** Prevent double submit: disable submit buttons once a form is sent. */
function initSubmitGuards() {
  document.querySelectorAll("form[data-guard]").forEach((form) => {
    form.addEventListener("submit", () => {
      form.querySelectorAll("button[type=submit]").forEach((b) => (b.disabled = true));
    });
  });
}

ready(() => {
  initPasswordToggles();
  initOtpInputs();
  initCountdowns();
  initToasts();
  initSubmitGuards();
});
