export function activateNavigation(pageId) {
  document.querySelectorAll('[data-nav]').forEach((link) => {
    link.classList.toggle('is-active', link.dataset.nav === pageId);
  });
}
