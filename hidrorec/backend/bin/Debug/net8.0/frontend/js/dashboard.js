import { initializePage } from './app.js';
import { dashboardApi } from './api.js';
import { renderMap } from './mapa.js';
import { createBadge, formatDateTime, humanizeEnum, renderError, renderLoading } from './utils.js';

await initializePage();

const countsTarget = document.getElementById('city-status-counts');
const attentionTarget = document.getElementById('attention-list');
const alertBanner = document.getElementById('alert-banner');
const mapTarget = document.getElementById('monitoring-map');

renderLoading(countsTarget);
renderLoading(attentionTarget);

async function loadDashboard() {
  try {
    const data = await dashboardApi.getResumo();

    document.getElementById('city-risk-title').textContent = data.statusCidade.nivelAtualRisco;
    document.getElementById('city-risk-description').textContent = `${data.statusCidade.alagamentosAtivos} alagamentos ativos e leitura consolidada dos ultimos reportes.`;
    document.getElementById('metric-tide').textContent = data.indicadores.mareAtual.valorPrincipal;
    document.getElementById('metric-tide-meta').textContent = data.indicadores.mareAtual.complemento;
    document.getElementById('metric-rain').textContent = data.indicadores.volumeChuva.valorPrincipal;
    document.getElementById('metric-rain-meta').textContent = data.indicadores.volumeChuva.complemento;

    countsTarget.innerHTML = `
      <article class="card metric-card"><span class="eyebrow">Normal</span><strong>${data.statusCidade.normal}</strong></article>
      <article class="card metric-card"><span class="eyebrow">Atencao</span><strong>${data.statusCidade.atencao}</strong></article>
      <article class="card metric-card"><span class="eyebrow">Alagamento</span><strong>${data.statusCidade.alagamento}</strong></article>
    `;

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

    if (data.bannerAlerta) {
      alertBanner.classList.remove('hidden');
      alertBanner.innerHTML = `<strong>${data.bannerAlerta}</strong><span>${data.statusCidade.nivelAtualRisco}</span>`;
    }

    renderMap(mapTarget, data.mapa);
  } catch (error) {
    renderError(countsTarget, error.message);
    renderError(attentionTarget, error.message);
  }
}

loadDashboard();
