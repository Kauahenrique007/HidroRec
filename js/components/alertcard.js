import { formatDateTime, formatSeverity } from '../utils/formatters.js';

export function renderAlertCard(alert) {
  return `
    <article class="alert-card alert-card--${alert.severity}">
      <header>
        <span class="badge badge--${alert.severity}">${formatSeverity(alert.severity)}</span>
        <time datetime="${alert.updatedAt}">${formatDateTime(alert.updatedAt)}</time>
      </header>
      <strong>${alert.title}</strong>
      <p>${alert.message}</p>
      <details class="alert-card__details">
        <summary>Detalhes do alerta</summary>
        <dl class="detail-list">
          <div><dt>Status</dt><dd>${alert.status || '--'}</dd></div>
          <div><dt>Origem</dt><dd>${alert.source || '--'}</dd></div>
          <div><dt>Bairro</dt><dd>${alert.neighborhoodName || 'Recife'}</dd></div>
        </dl>
      </details>
      <footer>
        <span>${alert.neighborhoodName || 'Recife'} - ${alert.source}</span>
        ${alert.territoryId ? `<a href="./detalhes.html?id=${alert.territoryId}" class="text-link">Ver territorio</a>` : ''}
      </footer>
    </article>
  `;
}
