import { CONFIG } from '../config.js';

export const statusService = {
  async getStatus() {
    const response = await fetch(`${CONFIG.APP_ORIGIN}/api/status`);
    if (!response.ok) {
      throw new Error('Nao foi possivel consultar o status da plataforma.');
    }

    return response.json();
  }
};
