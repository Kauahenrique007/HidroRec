import { CONFIG } from '../config.js';

let tokenProvider = () => null;

function sleep(ms) {
  return new Promise((resolve) => window.setTimeout(resolve, ms));
}

export function setTokenProvider(provider) {
  tokenProvider = provider;
}

class ApiService {
  constructor(baseURL) {
    this.baseURL = baseURL;
  }

  async request(endpoint, options = {}) {
    const {
      method = 'GET',
      body,
      headers = {},
      retry = CONFIG.RETRY_ATTEMPTS,
      withMeta = false
    } = options;

    const controller = new AbortController();
    const timeoutId = window.setTimeout(() => controller.abort(), CONFIG.REQUEST_TIMEOUT_MS);
    const token = tokenProvider();

    try {
      const response = await fetch(`${this.baseURL}${endpoint}`, {
        method,
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
          ...headers
        },
        body: body ? JSON.stringify(body) : undefined,
        signal: controller.signal
      });

      const payload = await this.parseResponse(response);

      if (!response.ok) {
        throw this.normalizeError(response, payload);
      }

      if (payload && typeof payload === 'object' && 'success' in payload) {
        return withMeta ? payload : payload.data;
      }

      return payload;
    } catch (error) {
      if (retry > 0 && (error.name === 'AbortError' || error.status >= 500 || !error.status)) {
        await sleep(500);
        return this.request(endpoint, { ...options, retry: retry - 1 });
      }

      throw error;
    } finally {
      window.clearTimeout(timeoutId);
    }
  }

  async parseResponse(response) {
    const contentType = response.headers.get('content-type') || '';
    if (!contentType.includes('application/json')) {
      return null;
    }

    return response.json();
  }

  normalizeError(response, payload) {
    const error = new Error(
      payload?.error?.message ||
      payload?.message ||
      `Falha na requisicao (${response.status})`
    );

    error.status = response.status;
    error.code = payload?.error?.code || 'API_ERROR';
    error.details = payload?.error?.details || null;
    return error;
  }

  get(endpoint, options = {}) {
    return this.request(endpoint, { ...options, method: 'GET' });
  }

  post(endpoint, body, options = {}) {
    return this.request(endpoint, { ...options, method: 'POST', body });
  }

  patch(endpoint, body, options = {}) {
    return this.request(endpoint, { ...options, method: 'PATCH', body });
  }
}

export const api = new ApiService(CONFIG.API_BASE_URL);
