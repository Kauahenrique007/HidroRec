import { initializePage } from './app.js';
import { dashboardApi } from './api.js';
import { renderMap } from './mapa.js';
import { createBadge, formatDateTime, humanizeEnum, renderEmpty, renderError, renderLoading } from './utils.js';

await initializePage();

const countsTarget = document.getElementById('city-status-counts');
const attentionTarget = document.getElementById('attention-list');
const alertBanner = document.getElementById('alert-banner');
const mapTarget = document.getElementById('monitoring-map');
const areasTarget = document.getElementById('areas-list');
const decisionsTarget = document.getElementById('decisions-list');
const assetsTarget = document.getElementById('assets-list');
const sourcesTarget = document.getElementById('sources-strip');

renderLoading(countsTarget);
renderLoading(attentionTarget);
renderLoading(areasTarget);
renderLoading(decisionsTarget);
renderLoading(assetsTarget);
renderLoading(sourcesTarget);

function createPriorityBadge(priority) {
  const normalized = String(priority || '').toLowerCase();
  const variant = normalized.includes('imedi') || normalized.includes('alta')
    ? 'alagamento'
    : normalized.includes('dire')
      ? 'atencao'
      : 'normal';

  return `<span class="status-badge status-badge--${variant}">${priority}</span>`;
}

async function loadDashboard() {
  try {
    const data = await dashboardApi.getResumo();
    const topArea = data.areasCriticas?.[0];
    const topDecision = data.decisoesOperacionais?.[0];

    document.getElementById('city-risk-title').textContent = data.statusCidade.nivelAtualRisco;
    document.getElementById('city-risk-description').textContent = data.statusCidade.situacaoAtual || (
      topArea
        ? `${topArea.nome} | score ${topArea.scoreRisco}`
        : `${data.statusCidade.alagamentosAtivos} ativos`
    );
    document.getElementById('city-official-reading').textContent = data.statusCidade.leituraOficial || '--';
    document.getElementById('metric-tide').textContent = data.indicadores.mareAtual.valorPrincipal;
    document.getElementById('metric-tide-meta').textContent = data.indicadores.mareAtual.complemento;
    document.getElementById('metric-rain').textContent = data.indicadores.volumeChuva.valorPrincipal;
    document.getElementById('metric-rain-meta').textContent = data.indicadores.volumeChuva.complemento;
    document.getElementById('operation-title').textContent = topDecision?.titulo || 'Monitoramento ativo';
    document.getElementById('operation-meta').textContent = data.statusCidade.estagioOperacional || topDecision?.acao || 'Sem acao prioritaria aberta.';
    document.getElementById('operation-ribbon-text').textContent = topArea
      ? `${topArea.nome} | ${topArea.nivelRisco}`
      : data.governanca?.diretrizExecutiva || 'Monitoramento ativo';
    document.getElementById('system-status-chip').textContent = data.statusCidade.estagioOperacional || 'Centro operacional ativo';
    document.getElementById('attention-meta').textContent = `${data.pontosAtencao.length} itens`;
    document.getElementById('areas-meta').textContent = `${data.areasCriticas.length} zonas`;
    document.getElementById('decisions-meta').textContent = `${data.decisoesOperacionais.length} acoes`;
    document.getElementById('assets-meta').textContent = `${data.ativosExpostos.length} ativos`;

    countsTarget.innerHTML = `
      <article class="card metric-card metric-card--status"><span class="eyebrow">Normal</span><strong>${data.statusCidade.normal}</strong></article>
      <article class="card metric-card metric-card--status"><span class="eyebrow">Atencao</span><strong>${data.statusCidade.atencao}</strong></article>
      <article class="card metric-card metric-card--status"><span class="eyebrow">Alagamento</span><strong>${data.statusCidade.alagamento}</strong></article>
    `;

    if (!data.pontosAtencao.length) {
      renderEmpty(attentionTarget, 'Sem eventos recentes.');
    } else {
      attentionTarget.innerHTML = data.pontosAtencao.map((item) => `
        <article class="list-item">
          <div class="list-item__row">
            <strong>${item.titulo}</strong>
            ${createBadge(item.severidade)}
          </div>
          <span>${item.area}</span>
          <small class="muted">${humanizeEnum(item.tipoOcorrencia)} | ${humanizeEnum(item.status)} | ${formatDateTime(item.dataOcorrencia)}</small>
        </article>
      `).join('');
    }

    if (!data.areasCriticas.length) {
      renderEmpty(areasTarget, 'Sem zonas priorizadas.');
    } else {
      areasTarget.innerHTML = data.areasCriticas.map((item) => `
        <article class="list-item compact-item">
          <div class="list-item__row">
            <strong>${item.nome}</strong>
            ${createBadge(item.nivelRisco)}
          </div>
          <span>${item.organizacao} | ${item.bairro || item.regiao}</span>
          <small class="muted">Score ${item.scoreRisco} | ${item.janelaCritica} | ${item.acaoSugerida}</small>
        </article>
      `).join('');
    }

    if (!data.decisoesOperacionais.length) {
      renderEmpty(decisionsTarget, 'Sem acoes abertas.');
    } else {
      decisionsTarget.innerHTML = data.decisoesOperacionais.map((item) => `
        <article class="list-item compact-item">
          <div class="list-item__row">
            <strong>${item.titulo}</strong>
            ${createPriorityBadge(item.prioridade)}
          </div>
          <span>${item.area}</span>
          <small class="muted">${item.janelaCritica} | ${item.acao}</small>
        </article>
      `).join('');
    }

    if (!data.ativosExpostos.length) {
      renderEmpty(assetsTarget, 'Sem ativos expostos.');
    } else {
      assetsTarget.innerHTML = data.ativosExpostos.map((item) => `
        <article class="list-item compact-item">
          <div class="list-item__row">
            <strong>${item.nome}</strong>
            ${createBadge(item.nivelRisco)}
          </div>
          <span>${item.organizacao} | ${item.area}</span>
          <small class="muted">${item.tipo} | score ${item.scoreRisco} | ${item.acaoSugerida}</small>
        </article>
      `).join('');
    }

    sourcesTarget.innerHTML = (data.fontes || []).map((item) => `
      <article class="card source-card">
        <span class="eyebrow">${item.nome}</span>
        <strong>${item.status}</strong>
        <small>${item.detalhe}</small>
      </article>
    `).join('');

    if (data.bannerAlerta) {
      alertBanner.classList.remove('hidden');
      alertBanner.classList.add('alert-banner--critical');
      alertBanner.innerHTML = `<strong>${data.bannerAlerta}</strong><span>${data.statusCidade.nivelAtualRisco}</span>`;
    }

    renderMap(mapTarget, data.mapa);
  } catch (error) {
    renderError(countsTarget, error.message);
    renderError(attentionTarget, error.message);
    renderError(areasTarget, error.message);
    renderError(decisionsTarget, error.message);
    renderError(assetsTarget, error.message);
    renderError(sourcesTarget, error.message);
  }
}

loadDashboard();
