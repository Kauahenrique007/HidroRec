import { api } from './api.js';

export const incidentsService = {
  list(params = {}) {
    const query = new URLSearchParams(params);
    return api.get(`/incidents?${query.toString()}`, { withMeta: true });
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
