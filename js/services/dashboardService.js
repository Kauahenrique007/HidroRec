import { api } from './api.js';

export const dashboardService = {
  getOverview() {
    return api.get('/dashboard/overview');
  }
};
