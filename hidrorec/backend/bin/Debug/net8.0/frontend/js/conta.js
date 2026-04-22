import { authApi, usersApi } from './api.js';
import { initializePage } from './app.js';
import { createBadge, formatDateTime, humanizeEnum, renderEmpty, renderError, renderLoading, showToast } from './utils.js';

const profileForm = document.getElementById('profile-form');
const submitButton = document.getElementById('profile-submit-button');
const metricsContainer = document.getElementById('conta-metrics');
const reportesList = document.getElementById('conta-reportes-list');
const reportesMetaLabel = document.getElementById('reportes-meta-label');
const profileChip = document.getElementById('conta-profile-chip');

function ensureAuthenticated() {
  const user = authApi.getStoredUser();
  if (!authApi.getToken() || !user) {
    window.location.href = './acesso.html';
    return null;
  }

  return user;
}

function fillProfile(user) {
  document.getElementById('profile-name').value = user.nome || '';
  document.getElementById('profile-phone').value = user.telefone || '';
  document.getElementById('profile-email').value = user.email || '';
  document.getElementById('profile-role').value = user.perfil || '';
  profileChip.textContent = user.perfil || 'Conta';
  const sideRole = document.getElementById('conta-side-role');
  if (sideRole) {
    sideRole.textContent = user.perfil || 'Conta';
  }
}

function renderMetrics(items) {
  metricsContainer.innerHTML = items.map((item) => `
    <article class="card metric-card">
      <span class="eyebrow">${item.title}</span>
      <strong>${item.value}</strong>
      <small>${item.caption}</small>
    </article>
  `).join('');
}

function renderReportes(items, totalItems) {
  if (!items.length) {
    renderEmpty(reportesList, 'Sem reportes.');
    reportesMetaLabel.textContent = '0 itens';
    return;
  }

  reportesMetaLabel.textContent = `${totalItems} itens`;
  reportesList.innerHTML = items.map((item) => `
    <article class="list-item">
      <div class="list-item__row">
        <strong>${item.titulo}</strong>
        ${createBadge(item.status)}
      </div>
      <small class="muted">${item.bairro}, ${item.regiao}</small>
      <div class="list-item__row">
        <span>${humanizeEnum(item.tipoOcorrencia)} | ${humanizeEnum(item.severidade)}</span>
        <small class="muted">${formatDateTime(item.dataOcorrencia)}</small>
      </div>
    </article>
  `).join('');
}

async function loadOwnReportes() {
  renderLoading(reportesList, 'Carregando...');

  try {
    const response = await usersApi.getMyReportes('page=1&pageSize=6');
    renderReportes(response.items || [], response.totalItems || 0);

    const items = response.items || [];
    const resolved = items.filter((item) => String(item.status).toLowerCase().includes('resol')).length;

    renderMetrics([
      { title: 'Reportes', value: String(response.totalItems || 0), caption: 'Total' },
      { title: 'Resolvidos', value: String(resolved), caption: 'Fechados' },
      { title: 'Perfil', value: authApi.getStoredUser()?.perfil || 'Conta', caption: 'Atual' },
      { title: 'Conta', value: 'Ativa', caption: 'Sessao' }
    ]);
  } catch (error) {
    renderError(reportesList, error.message || 'Erro.');
    renderMetrics([
      { title: 'Reportes', value: '--', caption: '--' },
      { title: 'Resolvidos', value: '--', caption: '--' },
      { title: 'Perfil', value: authApi.getStoredUser()?.perfil || '--', caption: 'Atual' },
      { title: 'Conta', value: 'Ativa', caption: 'Sessao' }
    ]);
  }
}

async function handleProfileSubmit(event) {
  event.preventDefault();
  submitButton.disabled = true;

  try {
    const updatedUser = await usersApi.updateMe({
      nome: document.getElementById('profile-name').value.trim(),
      telefone: document.getElementById('profile-phone').value.trim()
    });

    authApi.setStoredUser(updatedUser);
    fillProfile(updatedUser);
    showToast('Perfil atualizado.', 'normal');
  } catch (error) {
    showToast(error.message || 'Erro.', 'alagamento');
  } finally {
    submitButton.disabled = false;
  }
}

async function bootstrap() {
  await initializePage();
  const currentUser = ensureAuthenticated();
  if (!currentUser) {
    return;
  }

  fillProfile(currentUser);
  await loadOwnReportes();
  profileForm.addEventListener('submit', handleProfileSubmit);
}

bootstrap();
