import { clearSession, getSession, hydrateSession } from './services/authService.js';
import { activateNavigation } from './router.js';
import { statusService } from './services/statusService.js';

function ensureHeaderMeta(user) {
  const headerInner = document.querySelector('.app-header__inner');
  if (!headerInner) return;

  let meta = headerInner.querySelector('[data-header-meta]');
  if (!meta) {
    meta = document.createElement('div');
    meta.className = 'header-meta';
    meta.dataset.headerMeta = 'true';
    headerInner.append(meta);
  }

  meta.innerHTML = `
    <div class="session-chip session-chip--status" data-system-status>API em verificacao</div>
    ${
      user
        ? '<button class="button button--ghost header-logout" type="button" data-action="logout">Sair</button>'
        : '<a class="button button--ghost header-entry" href="./cadastro.html">Entrar na operacao</a>'
    }
  `;
}

async function hydrateSystemStatus() {
  const targets = document.querySelectorAll('[data-system-status]');
  if (!targets.length) return;

  try {
    const status = await statusService.getStatus();
    targets.forEach((target) => {
      target.textContent = `API ${status.status} - ${status.monitoring.mode}`;
    });
  } catch (error) {
    targets.forEach((target) => {
      target.textContent = 'API indisponivel';
    });
  }
}

function bindLogoutActions() {
  document.querySelectorAll('[data-action="logout"]').forEach((button) => {
    if (button.dataset.boundLogout === 'true') return;
    button.dataset.boundLogout = 'true';
    button.addEventListener('click', () => {
      clearSession();
      window.location.reload();
    });
  });
}

export async function initializeShell(pageId) {
  activateNavigation(pageId);

  const yearTarget = document.querySelector('[data-current-year]');
  if (yearTarget) {
    yearTarget.textContent = String(new Date().getFullYear());
  }

  bindLogoutActions();

  const session = getSession();
  const user = session ? await hydrateSession() : null;
  ensureHeaderMeta(user);

  document.querySelectorAll('[data-session-status]').forEach((target) => {
    target.textContent = user ? `${user.name} - ${user.role}` : 'Modo visitante';
    if (!user && target.tagName === 'A') {
      target.setAttribute('href', './cadastro.html');
      target.setAttribute('title', 'Entrar na operacao');
    }
  });

  bindLogoutActions();

  await hydrateSystemStatus();

  return user;
}
