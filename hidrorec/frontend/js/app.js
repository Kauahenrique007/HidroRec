import { authApi } from './api.js';
import { showToast, updateSessionChip } from './utils.js';

function bindLogout() {
  const logoutButton = document.getElementById('session-logout-button');
  if (!logoutButton || logoutButton.dataset.bound === 'true') {
    return;
  }

  logoutButton.dataset.bound = 'true';
  logoutButton.addEventListener('click', () => {
    authApi.logout();
    updateSessionChip();
    showToast('Sessao encerrada.', 'normal');
    if (window.location.pathname.endsWith('/admin.html') || window.location.pathname.endsWith('/conta.html')) {
      window.location.href = './acesso.html';
    }
  });
}

export async function initializePage() {
  bindLogout();
  updateSessionChip();

  if (!authApi.getToken()) {
    return;
  }

  try {
    await authApi.getCurrentUser();
  } catch {
    authApi.logout();
  } finally {
    updateSessionChip();
  }
}
