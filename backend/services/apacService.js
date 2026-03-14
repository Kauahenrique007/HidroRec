const axios = require('axios');
const { readDB } = require('../utils/fileUtils');

const APAC_API_URL = process.env.APAC_API_URL || 'https://api.apac.pe.gov.br/monitoramento';

async function getClimaFromAPAC() {
  try {
    const response = await axios.get(`${APAC_API_URL}/clima`, { timeout: 5000 });
    return {
      local: 'Recife',
      chuvaMm: response.data.chuva || 0,
      temperatura: response.data.temperatura || null,
      umidade: response.data.umidade || null,
      dataHora: new Date().toISOString(),
      fonte: 'APAC'
    };
  } catch (error) {
    console.error('Falha ao acessar APAC, usando fallback:', error.message);
    const data = await readDB();
    return data.clima.length > 0 
      ? data.clima[data.clima.length - 1] 
      : { local: 'Recife', chuvaMm: 0, dataHora: new Date().toISOString(), fonte: 'fallback' };
  }
}

async function getMareFromAPAC() {
  try {
    const response = await axios.get(`${APAC_API_URL}/mare`, { timeout: 5000 });
    return {
      local: 'Porto do Recife',
      nivel: response.data.nivel || 0,
      influencia: response.data.influencia || 'media',
      dataHora: new Date().toISOString(),
      fonte: 'APAC'
    };
  } catch (error) {
    console.error('Falha ao acessar dados de maré, usando fallback:', error.message);
    const data = await readDB();
    return data.mare.length > 0 
      ? data.mare[data.mare.length - 1] 
      : { local: 'Porto do Recife', nivel: 1.5, influencia: 'media', dataHora: new Date().toISOString(), fonte: 'fallback' };
  }
}

module.exports = { getClimaFromAPAC, getMareFromAPAC };