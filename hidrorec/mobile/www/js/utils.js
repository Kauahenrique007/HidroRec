import { authApi } from './api.js';

export const WATER_LEVELS = [
  { value: 'Pocas', label: 'Pocas', intensity: 20 },
  { value: 'Tornozelo', label: 'Tornozelo', intensity: 40 },
  { value: 'Joelho', label: 'Joelho', intensity: 58 },
  { value: 'Cintura', label: 'Cintura', intensity: 78 },
  { value: 'Peito', label: 'Peito', intensity: 100 }
];

export function formatDateTime(dateTime) {
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short'
  }).format(new Date(dateTime));
}

export function formatStatusClass(value) {
  const normalized = String(value || '').toLowerCase();
  if (normalized.includes('alag')) return 'alagamento';
  if (normalized.includes('aten')) return 'atencao';
  if (normalized.includes('crit')) return 'alagamento';
  return 'normal';
}

export function humanizeEnum(value) {
  return String(value || '')
    .replace(/([a-z])([A-Z])/g, '$1 $2')
    .replace('Critico', 'Critico')
    .replace('Atencao', 'Atencao')
    .replace('Em Analise', 'Em analise')
    .replace('Pocas', 'Pocas');
}

export function createBadge(label) {
  const variant = formatStatusClass(label);
  return `<span class="status-badge status-badge--${variant}">${humanizeEnum(label)}</span>`;
}

export function renderLoading(target, message = 'Carregando dados...') {
  target.innerHTML = `<div class="loading-state">${message}</div>`;
}

export function renderError(target, message = 'Nao foi possivel carregar os dados.') {
  target.innerHTML = `<div class="error-state">${message}</div>`;
}

export function renderEmpty(target, message = 'Nenhum dado encontrado.') {
  target.innerHTML = `<div class="empty-state">${message}</div>`;
}

export function showToast(message, type = 'normal') {
  const existing = document.getElementById('global-toast');
  if (existing) existing.remove();

  const toast = document.createElement('div');
  toast.id = 'global-toast';
  toast.className = `status-badge status-badge--${formatStatusClass(type)}`;
  toast.style.position = 'fixed';
  toast.style.top = '18px';
  toast.style.right = '18px';
  toast.style.zIndex = '120';
  toast.style.padding = '14px 18px';
  toast.style.boxShadow = '0 12px 28px rgba(11, 31, 58, 0.18)';
  toast.textContent = message;

  document.body.appendChild(toast);
  window.setTimeout(() => toast.remove(), 3200);
}

export async function fileToBase64(file) {
  if (!file) return null;

  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      const result = String(reader.result || '');
      resolve(result.includes(',') ? result.split(',')[1] : result);
    };
    reader.onerror = () => reject(new Error('Nao foi possivel ler a imagem.'));
    reader.readAsDataURL(file);
  });
}

export function getStoredSession() {
  return {
    token: authApi.getToken(),
    user: authApi.getStoredUser()
  };
}

export function isOperationalProfile(profile) {
  return ['Administrador', 'Gestor'].includes(String(profile || ''));
}

export function updateSessionChip() {
  const chip = document.getElementById('session-chip') || document.getElementById('admin-auth-status');
  const accessLink = document.getElementById('session-access-link');
  const accountNavLink = document.getElementById('account-nav-link');
  const logoutButton = document.getElementById('session-logout-button');
  const summary = document.getElementById('session-user-label');
  const profileBadge = document.getElementById('session-profile-badge');
  const adminShortcut = document.getElementById('admin-shortcut-link');
  const { token, user } = getStoredSession();

  if (!chip) {
    return;
  }

  if (token && user) {
    chip.textContent = `Sessao ativa`;
    summary && (summary.textContent = `${user.nome} | ${user.email}`);
    profileBadge && (profileBadge.textContent = user.perfil || 'Perfil');
    accessLink && (accessLink.textContent = 'Minha conta');
    accessLink && (accessLink.href = './conta.html');
    logoutButton && logoutButton.classList.remove('hidden');
    accountNavLink && accountNavLink.classList.remove('hidden');

    if (adminShortcut) {
      adminShortcut.classList.toggle('hidden', !isOperationalProfile(user.perfil));
    }

    if (chip.id === 'admin-auth-status') {
      chip.textContent = `${user.perfil}`;
    }
    return;
  }

  chip.textContent = chip.id === 'admin-auth-status' ? 'Acesso restrito' : 'Modo visitante';
  summary && (summary.textContent = 'Entre para acompanhar seus reportes e acessar o painel operacional.');
  profileBadge && (profileBadge.textContent = 'Visitante');
  accessLink && (accessLink.textContent = 'Entrar');
  accessLink && (accessLink.href = './acesso.html');
  logoutButton && logoutButton.classList.add('hidden');
  adminShortcut && adminShortcut.classList.add('hidden');
  accountNavLink && accountNavLink.classList.add('hidden');
}
