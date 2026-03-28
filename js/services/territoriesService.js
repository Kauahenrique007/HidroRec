import { api } from './api.js';
import { buildQueryString } from '../utils/helpers.js';

export const territoriesService = {
  list(params = {}) {
    const query = buildQueryString(params);
    return api.get(`/territories${query ? `?${query}` : ''}`, { withMeta: true });
  },
  getSummary(params = {}) {
    const query = buildQueryString(params);
    return api.get(`/territories/summary${query ? `?${query}` : ''}`);
  },
  getById(id) {
    return api.get(`/territories/${id}`);
  }
};
