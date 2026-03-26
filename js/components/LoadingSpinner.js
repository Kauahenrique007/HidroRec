export function renderLoadingBlock(label = 'Carregando painel operacional...') {
  return `
    <div class="loading-block" aria-live="polite">
      <span class="loading-block__dot"></span>
      <span>${label}</span>
    </div>
  `;
}
