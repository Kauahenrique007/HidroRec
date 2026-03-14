const { readDB } = require('../utils/fileUtils');

exports.getAll = async (req, res) => {
  try {
    const data = await readDB();
    res.json(data.areasRisco);
  } catch (error) {
    res.status(500).json({ erro: 'Erro ao buscar áreas de risco' });
  }
};