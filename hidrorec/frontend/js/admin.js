import { initializePage } from './app.js';
import { adminApi, authApi, reportApi } from './api.js';
import { createBadge, formatDateTime, getStoredSession, humanizeEnum, isOperationalProfile, renderError, renderLoading, showToast, updateSessionChip } from './utils.js';

await initializePage();

const loginForm = document.getElementById('admin-login-form');
const loginButton = document.getElementById('admin-login-button');
const metricsTarget = document.getElementById('admin-metrics');
const reportesTarget = document.getElementById('admin-reportes-table');
const auditoriaTarget = document.getElementById('admin-auditoria');
const logsTarget = document.getElementById('admin-logs');
const refreshButton = document.getElementById('admin-refresh-button');
const bairroFilter = document.getElementById('admin-bairro-filter');
const sessionChip = document.getElementById('admin-session-chip');
const sessionRole = document.getElementById('admin-session-role');
const sessionCopy = document.getElementById('admin-session-copy');

function renderAdminSession() {
  const { user } = getStoredSession();

  if (!user) {
    sessionChip.textContent = 'Nao autenticado';
    sessionRole.textContent = 'Visitante';
    sessionCopy.textContent = 'Use uma conta de Gestor ou Administrador para liberar o painel.';
    return false;
  }

  sessionChip.textContent = 'Sessao operacional';
  sessionRole.textContent = `${user.nome} | ${user.perfil}`;
  sessionCopy.textContent = isOperationalProfile(user.perfil)
    ? 'Seu perfil possui permissao para revisar reportes, atualizar status e consultar trilhas operacionais.'
    : 'Sua conta esta autenticada, mas o painel administrativo exige perfil Gestor ou Administrador.';

  return isOperationalProfile(user.perfil);
}

async function loadAdminPanel() {
  if (!authApi.getToken()) {
    metricsTarget.innerHTML = '<div class="empty-state">Faca login para carregar as metricas.</div>';
    reportesTarget.innerHTML = '<tr><td colspan="5">Faca login para consultar os reportes.</td></tr>';
    auditoriaTarget.innerHTML = '<div class="empty-state">Aguardando autenticacao.</div>';
    logsTarget.innerHTML = '<div class="empty-state">Aguardando autenticacao.</div>';
    return;
  }

  if (!renderAdminSession()) {
    metricsTarget.innerHTML = '<div class="empty-state">O painel administrativo exige perfil Gestor ou Administrador.</div>';
    reportesTarget.innerHTML = '<tr><td colspan="5">Acesso operacional insuficiente.</td></tr>';
    auditoriaTarget.innerHTML = '<div class="empty-state">Sem permissao operacional para auditoria.</div>';
    logsTarget.innerHTML = '<div class="empty-state">Sem permissao operacional para logs.</div>';
    return;
  }

  renderLoading(metricsTarget);

  try {
    const [metricas, reportes, auditoria, logs] = await Promise.all([
      adminApi.getMetricas(),
      adminApi.getReportes(bairroFilter.value ? `bairro=${encodeURIComponent(bairroFilter.value)}` : ''),
      adminApi.getAuditoria(),
      adminApi.getLogs()
    ]);

    metricsTarget.innerHTML = `
      <article class="card metric-card"><span class="eyebrow">Reportes</span><strong>${metricas.totalReportes}</strong><small>Total registrado</small></article>
      <article class="card metric-card"><span class="eyebrow">Pendentes</span><strong>${metricas.pendentes}</strong><small>Em triagem</small></article>
      <article class="card metric-card"><span class="eyebrow">Confirmados</span><strong>${metricas.confirmados}</strong><small>Eventos validados</small></article>
      <article class="card metric-card"><span class="eyebrow">Alertas ativos</span><strong>${metricas.alertasAtivos}</strong><small>Painel critico</small></article>
    `;

    reportesTarget.innerHTML = reportes.items.map((item) => `
      <tr>
        <td data-label="Titulo">
          <div class="admin-report-title">
            <strong>${item.titulo}</strong>
            <small>${humanizeEnum(item.tipoOcorrencia)} | ${formatDateTime(item.dataOcorrencia)}</small>
          </div>
        </td>
        <td data-label="Bairro">${item.bairro}</td>
        <td data-label="Severidade">${createBadge(item.severidade)}</td>
        <td data-label="Status">${humanizeEnum(item.status)}</td>
        <td data-label="Acao">
          <select class="input-control admin-status-select" data-status-id="${item.id}">
            <option value="Pendente" ${item.status === 'Pendente' ? 'selected' : ''}>Pendente</option>
            <option value="EmAnalise" ${item.status === 'EmAnalise' ? 'selected' : ''}>Em analise</option>
            <option value="Confirmado" ${item.status === 'Confirmado' ? 'selected' : ''}>Confirmado</option>
            <option value="Resolvido" ${item.status === 'Resolvido' ? 'selected' : ''}>Resolvido</option>
            <option value="Arquivado" ${item.status === 'Arquivado' ? 'selected' : ''}>Arquivado</option>
          </select>
        </td>
      </tr>
    `).join('');

    auditoriaTarget.innerHTML = auditoria.map((item) => `
      <article class="list-item">
        <strong>${item.acao}</strong>
        <small class="muted">${item.entidade} | ${item.usuario} | ${formatDateTime(item.dataCriacao)}</small>
      </article>
    `).join('');

    logsTarget.innerHTML = logs.map((item) => `
      <article class="list-item">
        <strong>${item.evento}</strong>
        <small class="muted">${item.nivel} | ${formatDateTime(item.dataCriacao)}</small>
      </article>
    `).join('');

    bindStatusUpdates();
  } catch (error) {
    renderError(metricsTarget, error.message);
    reportesTarget.innerHTML = `<tr><td colspan="5">${error.message}</td></tr>`;
    renderError(auditoriaTarget, error.message);
    renderError(logsTarget, error.message);
  }
}

function bindStatusUpdates() {
  document.querySelectorAll('[data-status-id]').forEach((select) => {
    select.addEventListener('change', async () => {
      try {
        await reportApi.updateStatus(select.dataset.statusId, {
          status: select.value,
          observacao: 'Atualizacao realizada pelo painel administrativo.'
        });
        showToast('Status atualizado.', 'normal');
      } catch (error) {
        showToast(error.message, 'alagamento');
      }
    });
  });
}

loginForm.addEventListener('submit', async (event) => {
  event.preventDefault();
  if (!loginForm.reportValidity()) {
    return;
  }

  loginButton.disabled = true;
  loginButton.textContent = 'Autenticando...';

  try {
    const response = await authApi.login({
      email: document.getElementById('admin-email').value.trim(),
      senha: document.getElementById('admin-password').value
    });

    updateSessionChip();
    renderAdminSession();

    if (!isOperationalProfile(response.usuario.perfil)) {
      showToast('Sua conta nao possui perfil operacional para o admin.', 'atencao');
      return;
    }

    showToast('Sessao administrativa iniciada.', 'normal');
    await loadAdminPanel();
  } catch (error) {
    showToast(error.message, 'alagamento');
  } finally {
    loginButton.disabled = false;
    loginButton.textContent = 'Acessar painel';
  }
});

refreshButton.addEventListener('click', loadAdminPanel);

renderAdminSession();
loadAdminPanel();
