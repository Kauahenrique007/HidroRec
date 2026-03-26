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
      <footer>
        <span>${alert.neighborhoodName || 'Recife'} - ${alert.source}</span>
        ${alert.territoryId ? `<a href="./detalhes.html?id=${alert.territoryId}" class="text-link">Ver territorio</a>` : ''}
      </footer>
    </article>
  `;
}
