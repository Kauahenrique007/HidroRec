import { api } from './api.js';

export const monitoringService = {
  getOverview(refresh = false) {
    return api.get(`/monitoring/overview${refresh ? '?refresh=true' : ''}`);
  },
  getTimeline(limit = 12) {
    return api.get(`/monitoring/timeline?limit=${limit}`);
  },
  getIntegrations() {
    return api.get('/monitoring/integrations');
  }
};
