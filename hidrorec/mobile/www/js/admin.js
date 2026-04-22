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
const dataTracesTarget = document.getElementById('admin-data-traces');
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
    sessionCopy.textContent = 'Use conta operacional.';
    return false;
  }

  sessionChip.textContent = 'Sessao operacional';
  sessionRole.textContent = `${user.nome} | ${user.perfil}`;
  sessionCopy.textContent = isOperationalProfile(user.perfil) ? 'Acesso liberado.' : 'Sem permissao.';

  return isOperationalProfile(user.perfil);
}

async function loadAdminPanel() {
  if (!authApi.getToken()) {
    metricsTarget.innerHTML = '<div class="empty-state">Sem sessao.</div>';
    reportesTarget.innerHTML = '<tr><td colspan="5">Sem sessao.</td></tr>';
    auditoriaTarget.innerHTML = '<div class="empty-state">Sem sessao.</div>';
    logsTarget.innerHTML = '<div class="empty-state">Sem sessao.</div>';
    dataTracesTarget.innerHTML = '<div class="empty-state">Sem sessao.</div>';
    return;
  }

  if (!renderAdminSession()) {
    metricsTarget.innerHTML = '<div class="empty-state">Sem permissao.</div>';
    reportesTarget.innerHTML = '<tr><td colspan="5">Sem permissao.</td></tr>';
    auditoriaTarget.innerHTML = '<div class="empty-state">Sem permissao.</div>';
    logsTarget.innerHTML = '<div class="empty-state">Sem permissao.</div>';
    dataTracesTarget.innerHTML = '<div class="empty-state">Sem permissao.</div>';
    return;
  }

  renderLoading(metricsTarget);

  try {
    const [metricas, reportes, auditoria, logs, dataTraces] = await Promise.all([
      adminApi.getMetricas(),
      adminApi.getReportes(bairroFilter.value ? `bairro=${encodeURIComponent(bairroFilter.value)}` : ''),
      adminApi.getAuditoria(),
      adminApi.getLogs(),
      adminApi.getLogs('contexto=data-fusion')
    ]);

    metricsTarget.innerHTML = `
      <article class="card metric-card"><span class="eyebrow">Reportes</span><strong>${metricas.totalReportes}</strong></article>
      <article class="card metric-card"><span class="eyebrow">Pendentes</span><strong>${metricas.pendentes}</strong></article>
      <article class="card metric-card"><span class="eyebrow">Confirmados</span><strong>${metricas.confirmados}</strong></article>
      <article class="card metric-card"><span class="eyebrow">Alertas</span><strong>${metricas.alertasAtivos}</strong></article>
      <article class="card metric-card"><span class="eyebrow">Orgs</span><strong>${metricas.organizacoesAtivas}</strong></article>
      <article class="card metric-card"><span class="eyebrow">Areas</span><strong>${metricas.areasMonitoradas}</strong></article>
      <article class="card metric-card"><span class="eyebrow">Ativos</span><strong>${metricas.ativosMonitorados}</strong></article>
      <article class="card metric-card"><span class="eyebrow">Fusion 24h</span><strong>${metricas.leiturasFusionCriticas24h}</strong></article>
      <article class="card metric-card"><span class="eyebrow">Fontes degr.</span><strong>${metricas.fontesDegradadas24h}</strong></article>
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
        <small class="muted">${formatDateTime(item.dataCriacao)}</small>
      </article>
    `).join('');

    logsTarget.innerHTML = logs.map((item) => `
      <article class="list-item">
        <strong>${item.evento}</strong>
        <small class="muted">${item.nivel}</small>
      </article>
    `).join('');

    if (!dataTraces.length) {
      dataTracesTarget.innerHTML = '<div class="empty-state">Sem rastros recentes.</div>';
    } else {
      dataTracesTarget.innerHTML = dataTraces.map((item) => `
        <article class="list-item">
          <div class="list-item__row">
            <strong>${item.evento}</strong>
            ${createBadge(item.nivel)}
          </div>
          <span>${item.mensagem}</span>
          <small class="muted">${formatDateTime(item.dataCriacao)}</small>
        </article>
      `).join('');
    }

    bindStatusUpdates();
  } catch (error) {
    renderError(metricsTarget, error.message);
    reportesTarget.innerHTML = `<tr><td colspan="5">${error.message}</td></tr>`;
    renderError(auditoriaTarget, error.message);
    renderError(logsTarget, error.message);
    renderError(dataTracesTarget, error.message);
  }
}

function bindStatusUpdates() {
  document.querySelectorAll('[data-status-id]').forEach((select) => {
    select.addEventListener('change', async () => {
      try {
        await reportApi.updateStatus(select.dataset.statusId, {
          status: select.value,
          observacao: 'Atualizacao pelo admin.'
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
  loginButton.textContent = 'Entrando...';

  try {
    const response = await authApi.login({
      email: document.getElementById('admin-email').value.trim(),
      senha: document.getElementById('admin-password').value
    });

    updateSessionChip();
    renderAdminSession();

    if (!isOperationalProfile(response.usuario.perfil)) {
      showToast('Sem permissao.', 'atencao');
      return;
    }

    showToast('Sessao iniciada.', 'normal');
    await loadAdminPanel();
  } catch (error) {
    showToast(error.message, 'alagamento');
  } finally {
    loginButton.disabled = false;
    loginButton.textContent = 'Entrar';
  }
});

refreshButton.addEventListener('click', loadAdminPanel);

renderAdminSession();
loadAdminPanel();
