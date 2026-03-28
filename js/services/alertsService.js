import { api } from './api.js';
import { buildQueryString } from '../utils/helpers.js';

export const alertsService = {
  list(params = {}) {
    const query = buildQueryString(params);
    return api.get(`/alerts${query ? `?${query}` : ''}`, { withMeta: true });
  },
  getSummary(params = {}) {
    const query = buildQueryString(params);
    return api.get(`/alerts/summary${query ? `?${query}` : ''}`);
  },
  getById(id) {
    return api.get(`/alerts/${id}`);
  }
};
