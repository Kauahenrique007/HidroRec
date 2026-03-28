import { api } from './api.js';
import { buildQueryString } from '../utils/helpers.js';

export const incidentsService = {
  list(params = {}) {
    const query = buildQueryString(params);
    return api.get(`/incidents${query ? `?${query}` : ''}`, { withMeta: true });
  },
  getSummary(params = {}) {
    const query = buildQueryString(params);
    return api.get(`/incidents/summary${query ? `?${query}` : ''}`);
  },
  getById(id) {
    return api.get(`/incidents/${id}`);
  },
  createPublicReport(payload) {
    return api.post('/incidents/public-report', payload);
  },
  createOperational(payload) {
    return api.post('/incidents/operations', payload);
  },
  updateStatus(id, payload) {
    return api.patch(`/incidents/${id}/status`, payload);
  }
};
