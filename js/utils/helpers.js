export function qs(selector, scope = document) {
  return scope.querySelector(selector);
}

export function qsa(selector, scope = document) {
  return Array.from(scope.querySelectorAll(selector));
}

export function renderEmptyState(message, detail = '') {
  return `
    <div class="empty-state">
      <strong>${message}</strong>
      ${detail ? `<p>${detail}</p>` : ''}
    </div>
  `;
}

export function renderErrorState(message) {
  return `
    <div class="error-state">
      <strong>Falha ao carregar</strong>
      <p>${message}</p>
    </div>
  `;
}

export function serializeForm(form) {
  return Object.fromEntries(new FormData(form).entries());
}

export function setFeedback(target, { type = 'info', title, message }) {
  if (!target) return;
  target.className = `feedback ${type}`;
  target.innerHTML = `<strong>${title}</strong><span>${message}</span>`;
}

export function clearFeedback(target) {
  if (!target) return;
  target.className = 'feedback';
  target.innerHTML = '';
}

export function toggleVisibility(element, visible) {
  if (!element) return;
  element.hidden = !visible;
}

export function getQueryParam(name) {
  const params = new URLSearchParams(window.location.search);
  return params.get(name);
}

export function setButtonBusy(button, busy, labels = {}) {
  if (!button) return;
  if (!button.dataset.defaultLabel) {
    button.dataset.defaultLabel = labels.defaultLabel || button.textContent;
  }

  button.disabled = busy;
  button.textContent = busy
    ? labels.busyLabel || 'Processando...'
    : labels.defaultLabel || button.dataset.defaultLabel;
}
