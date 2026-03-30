import { initializePage } from './app.js';
import { previsoesApi } from './api.js';
import { renderError, renderLoading } from './utils.js';

initializePage();

const cardsTarget = document.getElementById('forecast-cards');
const windowsTarget = document.getElementById('critical-windows');

renderLoading(cardsTarget);
renderLoading(windowsTarget);

async function loadForecast() {
  try {
    const data = await previsoesApi.getResumo();

    cardsTarget.innerHTML = data.indicadores.map((card) => `
      <article class="card metric-card">
        <span class="eyebrow">${card.titulo}</span>
        <strong>${card.valor}</strong>
        <small>${card.status} • ${card.descricao}</small>
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

loadForecast();
