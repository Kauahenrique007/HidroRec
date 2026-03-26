import { api } from './api.js';

export const climateService = {
  async getCurrentClimate() {
    const overview = await api.get('/monitoring/overview');
    return overview.climate;
  }
};
