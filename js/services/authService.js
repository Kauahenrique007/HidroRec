import { CONFIG } from '../config.js';
import { api, setTokenProvider } from './api.js';

function getStoredSession() {
  const raw = window.localStorage.getItem(CONFIG.STORAGE_KEYS.session);
  return raw ? JSON.parse(raw) : null;
}

function persistSession(session) {
  window.localStorage.setItem(CONFIG.STORAGE_KEYS.session, JSON.stringify(session));
}

export function getSession() {
  return getStoredSession();
}

export function getToken() {
  return getStoredSession()?.token || null;
}

export function clearSession() {
  window.localStorage.removeItem(CONFIG.STORAGE_KEYS.session);
}

export async function login(credentials) {
  const payload = await api.post('/auth/login', credentials);
  persistSession(payload);
  return payload;
}

export async function hydrateSession() {
  const session = getStoredSession();
  if (!session?.token) return null;

  try {
    const user = await api.get('/auth/me');
    const nextSession = { ...session, user };
    persistSession(nextSession);
    return user;
  } catch (error) {
    clearSession();
    return null;
  }
}

setTokenProvider(getToken);
