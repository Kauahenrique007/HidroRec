import { initializePage } from './app.js';
import { authApi } from './api.js';
import { getStoredSession, isOperationalProfile, showToast } from './utils.js';

await initializePage();

const loginForm = document.getElementById('login-form');
const registerForm = document.getElementById('register-form');
const loginButton = document.getElementById('login-submit-button');
const registerButton = document.getElementById('register-submit-button');
const sessionTitle = document.getElementById('access-session-title');
const sessionDescription = document.getElementById('access-session-description');

function renderSessionBox() {
  const { user } = getStoredSession();

  if (!user) {
    sessionTitle.textContent = 'Nao autenticado';
    sessionDescription.textContent = 'Sem sessao ativa.';
    return;
  }

  sessionTitle.textContent = `${user.nome} | ${user.perfil}`;
  sessionDescription.textContent = isOperationalProfile(user.perfil) ? 'Acesso liberado.' : 'Conta ativa.';
}

function bindTabs() {
  document.querySelectorAll('[data-auth-tab]').forEach((button) => {
    button.addEventListener('click', () => {
      const tab = button.dataset.authTab;
      document.querySelectorAll('[data-auth-tab]').forEach((item) => item.classList.toggle('is-active', item === button));
      loginForm.classList.toggle('hidden', tab !== 'login');
      registerForm.classList.toggle('hidden', tab !== 'register');
    });
  });
}

loginForm.addEventListener('submit', async (event) => {
  event.preventDefault();
  if (!loginForm.reportValidity()) {
    return;
  }

  loginButton.disabled = true;
  loginButton.textContent = 'Entrando...';

  try {
    const response = await authApi.login({
      email: document.getElementById('login-email').value.trim(),
      senha: document.getElementById('login-password').value
    });

    showToast('Sessao iniciada.', 'normal');
    renderSessionBox();

    window.setTimeout(() => {
      window.location.href = isOperationalProfile(response.usuario.perfil) ? './admin.html' : './index.html';
    }, 700);
  } catch (error) {
    showToast(error.message, 'alagamento');
  } finally {
    loginButton.disabled = false;
    loginButton.textContent = 'Entrar';
  }
});

registerForm.addEventListener('submit', async (event) => {
  event.preventDefault();
  if (!registerForm.reportValidity()) {
    return;
  }

  registerButton.disabled = true;
  registerButton.textContent = 'Criando...';

  try {
    await authApi.register({
      nome: document.getElementById('register-name').value.trim(),
      email: document.getElementById('register-email').value.trim(),
      senha: document.getElementById('register-password').value,
      telefone: document.getElementById('register-phone').value.trim()
    });

    showToast('Conta criada.', 'normal');
    renderSessionBox();
    window.setTimeout(() => {
      window.location.href = './index.html';
    }, 700);
  } catch (error) {
    showToast(error.message, 'alagamento');
  } finally {
    registerButton.disabled = false;
    registerButton.textContent = 'Criar conta';
  }
});

bindTabs();
renderSessionBox();
