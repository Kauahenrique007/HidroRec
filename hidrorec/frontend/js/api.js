function unique(items) {
  return [...new Set(items.filter(Boolean))];
}

function resolveApiOrigins() {
  const configuredBase = window.HIDROREC_API_BASE_URL?.trim();
  if (configuredBase) {
    return [configuredBase.replace(/\/+$/, '')];
  }

  const { protocol, hostname, port, origin } = window.location;
  const isFileProtocol = protocol === 'file:';
  const localApiOrigins = unique([
    hostname ? `${protocol}//${hostname}:8080` : '',
    'http://localhost:8080',
    'http://127.0.0.1:8080'
  ]);

  if (isFileProtocol) {
    return localApiOrigins;
  }

  if (port && port !== '8080') {
    return unique([...localApiOrigins, origin]);
  }

  return unique([origin, ...localApiOrigins]);
}

const API_BASE_CANDIDATES = resolveApiOrigins().map((origin) => `${origin}/api`);
const TOKEN_KEY = 'hidrorec.token';
const USER_KEY = 'hidrorec.user';

function getStorage() {
  return window.sessionStorage;
}

function migrateLegacyStorage() {
  const sessionStorage = getStorage();
  const legacyToken = window.localStorage.getItem(TOKEN_KEY);
  const legacyUser = window.localStorage.getItem(USER_KEY);

  if (legacyToken && !sessionStorage.getItem(TOKEN_KEY)) {
    sessionStorage.setItem(TOKEN_KEY, legacyToken);
  }

  if (legacyUser && !sessionStorage.getItem(USER_KEY)) {
    sessionStorage.setItem(USER_KEY, legacyUser);
  }

  window.localStorage.removeItem(TOKEN_KEY);
  window.localStorage.removeItem(USER_KEY);
}

async function parsePayload(response) {
  const contentType = response.headers.get('content-type') || '';

  if (contentType.includes('application/json')) {
    return response.json().catch(() => null);
  }

  return response.text().catch(() => null);
}

function shouldRetryWithAnotherOrigin(status) {
  return [0, 404, 405].includes(status);
}

function getErrorMessage(payload, status) {
  if (!payload) {
    return `Falha na requisicao (${status}).`;
  }

  if (typeof payload === 'string' && payload.trim()) {
    return payload;
  }

  if (payload.message) {
    return payload.message;
  }

  if (payload.title && payload.errors) {
    const details = Object.values(payload.errors)
      .flat()
      .filter(Boolean)
      .join(' ');

    return details || payload.title;
  }

  if (payload.errors && Array.isArray(payload.errors)) {
    const details = payload.errors
      .map((error) => error?.mensagem || error?.message || '')
      .filter(Boolean)
      .join(' ');

    if (details) {
      return details;
    }
  }

  return `Falha na requisicao (${status}).`;
}

async function request(path, options = {}) {
  migrateLegacyStorage();

  const storage = getStorage();
  const token = storage.getItem(TOKEN_KEY);
  const hasJsonBody = options.body && !(options.body instanceof FormData);
  const headers = {
    ...(hasJsonBody ? { 'Content-Type': 'application/json' } : {}),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(options.headers || {})
  };

  let lastError = null;

  for (let index = 0; index < API_BASE_CANDIDATES.length; index += 1) {
    const apiBaseUrl = API_BASE_CANDIDATES[index];

    try {
      const response = await fetch(`${apiBaseUrl}${path}`, {
        ...options,
        headers,
        mode: 'cors',
        credentials: 'omit'
      });

      const payload = await parsePayload(response);

      if (response.ok) {
        return payload?.data ?? payload;
      }

      lastError = new Error(getErrorMessage(payload, response.status));

      if (shouldRetryWithAnotherOrigin(response.status) && index < API_BASE_CANDIDATES.length - 1) {
        continue;
      }

      throw lastError;
    } catch (error) {
      lastError = error;

      if (index >= API_BASE_CANDIDATES.length - 1) {
        break;
      }
    }
  }

  throw lastError || new Error('Falha na comunicacao com a API.');
}

export const authApi = {
  async login(credentials) {
    const data = await request('/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials)
    });
    const storage = getStorage();
    storage.setItem(TOKEN_KEY, data.token);
    storage.setItem(USER_KEY, JSON.stringify(data.usuario));
    return data;
  },
  async register(payload) {
    const data = await request('/auth/register', {
      method: 'POST',
      body: JSON.stringify(payload)
    });
    const storage = getStorage();
    storage.setItem(TOKEN_KEY, data.token);
    storage.setItem(USER_KEY, JSON.stringify(data.usuario));
    return data;
  },
  async getCurrentUser() {
    const data = await request('/auth/me');
    getStorage().setItem(USER_KEY, JSON.stringify(data));
    return data;
  },
  logout() {
    const storage = getStorage();
    storage.removeItem(TOKEN_KEY);
    storage.removeItem(USER_KEY);
  },
  getToken() {
    migrateLegacyStorage();
    return getStorage().getItem(TOKEN_KEY);
  },
  getStoredUser() {
    migrateLegacyStorage();

    const value = getStorage().getItem(USER_KEY);
    if (!value) {
      return null;
    }

    try {
      return JSON.parse(value);
    } catch {
      getStorage().removeItem(USER_KEY);
      return null;
    }
  }
};

export const dashboardApi = {
  getResumo: () => request('/dashboard/resumo'),
  getMapa: () => request('/dashboard/mapa'),
  getPontosAtencao: () => request('/dashboard/pontos-atencao')
};

export const reportApi = {
  create: (body) => request('/reportes', { method: 'POST', body: JSON.stringify(body) }),
  list: (params = '') => request(`/reportes${params ? `?${params}` : ''}`),
  updateStatus: (id, body) => request(`/reportes/${id}/status`, { method: 'PATCH', body: JSON.stringify(body) })
};

export const alertsApi = {
  getAll: (criticidade = '') => request(`/alertas/ativos${criticidade ? `?criticidade=${encodeURIComponent(criticidade)}` : ''}`)
};

export const previsoesApi = {
  getResumo: () => request('/previsoes'),
  getChuva: () => request('/previsoes/chuva'),
  getMare: () => request('/previsoes/mare')
};

export const adminApi = {
  getMetricas: () => request('/admin/metricas'),
  getReportes: (params = '') => request(`/admin/reportes${params ? `?${params}` : ''}`),
  getAuditoria: () => request('/admin/auditoria'),
  getLogs: () => request('/admin/logs')
};
