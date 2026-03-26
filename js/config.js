const SERVER_ORIGIN = window.location.protocol === 'file:' ? 'http://localhost:3000' : window.location.origin;

export const CONFIG = {
  APP_ORIGIN: SERVER_ORIGIN,
  API_BASE_URL: `${SERVER_ORIGIN}/api/v1`,
  REQUEST_TIMEOUT_MS: 8000,
  RETRY_ATTEMPTS: 1,
  STORAGE_KEYS: {
    session: 'hidrorec.session'
  }
};
