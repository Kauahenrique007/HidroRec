const climateService = {
  async getCurrentClimate() {
    try {
      return await api.get('/clima');
    } catch (error) {
      console.error('Erro ao buscar clima:', error);
      return { chuvaMm: 0, dataHora: new Date().toISOString(), fonte: 'erro' };
    }
  }
};