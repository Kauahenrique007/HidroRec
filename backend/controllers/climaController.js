const climateService = require('../services/climateService');

exports.getCurrent = async (req, res) => {
  try {
    const clima = await climateService.getCurrentClimate();
    res.json(clima);
  } catch (error) {
    res.status(500).json({ erro: 'Erro ao buscar clima' });
  }
};