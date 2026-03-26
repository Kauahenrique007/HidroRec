import { api } from './api.js';

export const territoriesService = {
  list(params = {}) {
    const query = new URLSearchParams(params);
    return api.get(`/territories?${query.toString()}`, { withMeta: true });
  },
  getById(id) {
    return api.get(`/territories/${id}`);
  }
};
