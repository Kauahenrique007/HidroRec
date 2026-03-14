const { readDB } = require('../utils/fileUtils');

exports.getAll = async (req, res) => {
  try {
    const data = await readDB();
    const { status } = req.query;
    let alertas = data.alertas;
    if (status) alertas = alertas.filter(a => a.status === status);
    res.json(alertas);
  } catch (error) {
    res.status(500).json({ erro: 'Erro ao buscar alertas' });
  }
};

exports.getById = async (req, res) => {
  try {
    const data = await readDB();
    const alerta = data.alertas.find(a => a.id == req.params.id);
    if (!alerta) return res.status(404).json({ erro: 'Alerta não encontrado' });
    res.json(alerta);
  } catch (error) {
    res.status(500).json({ erro: 'Erro ao buscar alerta' });
  }
};