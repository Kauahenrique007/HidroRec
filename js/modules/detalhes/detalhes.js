import { initializeShell } from '../../app.js';
import { renderAlertCard } from '../../components/alertcard.js';
import { renderLoadingBlock } from '../../components/LoadingSpinner.js';
import { territoriesService } from '../../services/territoriesService.js';
import { getQueryParam, renderErrorState } from '../../utils/helpers.js';
import { formatDateTime, formatRisk } from '../../utils/formatters.js';

function renderExplanation(explanation) {
  return explanation.map((item) => `
    <article class="explanation-card">
      <span class="section-title__eyebrow">${item.label}</span>
      <strong>${item.value}</strong>
      <p>Contribuicao no score: ${item.contribution}</p>
    </article>
  `).join('');
}

async function initDetalhesPage() {
  await initializeShell('lista');

  const id = getQueryParam('id');
  const heroTitle = document.getElementById('detail-title');
  const heroMeta = document.getElementById('detail-meta');
  const scoreTarget = document.getElementById('detail-score');
  const recommendationTarget = document.getElementById('detail-recommendation');
  const explanationTarget = document.getElementById('detail-explanation');
  const alertsTarget = document.getElementById('detail-alerts');
  const incidentsTarget = document.getElementById('detail-incidents');

  if (!id) {
    const failure = renderErrorState('Territorio nao informado.');
    explanationTarget.innerHTML = failure;
    alertsTarget.innerHTML = failure;
    incidentsTarget.innerHTML = failure;
    return;
  }

  const loading = renderLoadingBlock('Carregando territorio...');
  explanationTarget.innerHTML = loading;
  alertsTarget.innerHTML = loading;
  incidentsTarget.innerHTML = `<tr><td colspan="4">${loading}</td></tr>`;

  try {
    const territory = await territoriesService.getById(id);
    heroTitle.textContent = territory.name;
    heroMeta.textContent = `${territory.neighborhoodName} - ${territory.address}`;
    scoreTarget.innerHTML = `
      <span class="badge badge--${territory.risk.level}">${formatRisk(territory.risk.level)}</span>
      <strong>${territory.risk.score}</strong>
    `;
    recommendationTarget.textContent = territory.risk.recommendation;
    explanationTarget.innerHTML = renderExplanation(territory.risk.explanation);
    alertsTarget.innerHTML = territory.alerts.length
      ? territory.alerts.map(renderAlertCard).join('')
      : '<div class="empty-state"><strong>Sem alertas ativos para este territorio.</strong></div>';
    incidentsTarget.innerHTML = territory.incidents.length
      ? territory.incidents.map((incident) => `
          <tr>
            <td>${incident.address}</td>
            <td>${incident.type}</td>
            <td>${incident.status}</td>
            <td>${formatDateTime(incident.updatedAt)}</td>
          </tr>
        `).join('')
      : '<tr><td colspan="4">Nenhuma ocorrencia recente associada.</td></tr>';
  } catch (error) {
    const failure = renderErrorState(error.message);
    explanationTarget.innerHTML = failure;
    alertsTarget.innerHTML = failure;
    incidentsTarget.innerHTML = `<tr><td colspan="4">${failure}</td></tr>`;
  }
}

initDetalhesPage();
