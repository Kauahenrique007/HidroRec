export function renderStatCard({ label, value, hint = '', tone = 'neutral' }) {
  return `
    <article class="stat-card stat-card--${tone}">
      <span class="stat-card__label">${label}</span>
      <strong class="stat-card__value">${value}</strong>
      <small class="stat-card__hint">${hint}</small>
    </article>
  `;
}
