import { api } from './api.js';

export const alertsService = {
  list(params = {}) {
    const query = new URLSearchParams(params);
    return api.get(`/alerts?${query.toString()}`, { withMeta: true });
  },
  getById(id) {
    return api.get(`/alerts/${id}`);
  }
};
