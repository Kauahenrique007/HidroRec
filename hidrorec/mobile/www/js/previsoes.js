import { initializePage } from './app.js';
import { previsoesApi } from './api.js';
import { formatDateTime, renderError, renderLoading } from './utils.js';

initializePage();

const cardsTarget = document.getElementById('forecast-cards');
const windowsTarget = document.getElementById('critical-windows');

renderLoading(cardsTarget);
renderLoading(windowsTarget);

async function loadForecast() {
  try {
    const data = await previsoesApi.getResumo();

    document.getElementById('forecast-current-title').textContent = data.situacaoAtual;
    document.getElementById('forecast-current-reading').textContent = data.leituraOficial;
    document.getElementById('forecast-current-recommendation').textContent = data.recomendacao;
    document.getElementById('forecast-next-day').textContent = data.janela24h;
    document.getElementById('forecast-updated-at').textContent = formatDateTime(data.atualizadoEm);

    cardsTarget.innerHTML = data.indicadores.map((card) => `
      <article class="card metric-card ${resolveForecastCardClass(card.titulo)}">
        <span class="eyebrow">${card.titulo}</span>
        <strong>${card.valor}</strong>
        <small>${card.status}</small>
        <p class="forecast-card__description">${card.descricao || ''}</p>
      </article>
    `).join('');

    windowsTarget.innerHTML = data.periodosCriticos.map((windowItem) => `
      <article class="list-item">
        <div class="list-item__row">
          <strong>${windowItem.periodo}</strong>
          <span class="status-badge status-badge--${windowItem.nivelRisco.toLowerCase().includes('alto') ? 'alagamento' : 'atencao'}">${windowItem.nivelRisco}</span>
        </div>
        <small class="muted">${windowItem.justificativa}</small>
      </article>
    `).join('');
  } catch (error) {
    renderError(cardsTarget, error.message);
    renderError(windowsTarget, error.message);
  }
}

function resolveForecastCardClass(title) {
  const normalized = String(title || '').toLowerCase();

  if (
    normalized.includes('chuva') ||
    normalized.includes('proximas 6h') ||
    normalized.includes('proximas 24h') ||
    normalized.includes('pico horario')
  ) {
    return 'metric-card--rain';
  }

  if (normalized.includes('mare')) {
    return 'metric-card--tide';
  }

  return '';
}

loadForecast();
