const { readDB } = require('../utils/fileUtils');
const alertsService = require('../services/alertsService');
const climateService = require('../services/climateService');
const tideService = require('../services/tideService');

exports.getResumo = async (req, res) => {
  try {
    const data = await readDB();
    const alertasAtivos = await alertsService.getActiveAlerts();
    const ocorrencias24h = data.ocorrencias.filter(o => {
      const dataOcorrencia = new Date(o.dataHora);
      const agora = new Date();
      const diffMs = agora - dataOcorrencia;
      const diffHoras = diffMs / (1000 * 60 * 60);
      return diffHoras <= 24;
    }).length;

    const clima = await climateService.getCurrentClimate();
    const mare = await tideService.getCurrentTide();

    const pontosCriticos = data.areasRisco.filter(area => area.nivel === 'alto').slice(0, 5);

    res.json({
      alertasAtivos: alertasAtivos.length,
      ocorrencias24h,
      chuva: clima.chuvaMm,
      mare: mare.nivel,
      mareInfluencia: mare.influencia,
      pontosCriticos,
      ultimaAtualizacao: new Date().toISOString()
    });
  } catch (error) {
    console.error(error);
    res.status(500).json({ erro: 'Erro ao montar dashboard' });
  }
};