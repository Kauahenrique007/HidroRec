import { api } from './api.js';

export const tideService = {
  async getCurrentTide() {
    const overview = await api.get('/monitoring/overview');
    return overview.tide;
  }
};
