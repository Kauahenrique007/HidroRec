const express = require('express');
const router = express.Router();

const areasRisco = [
  {
    id: 1,
    bairro: 'Boa Viagem',
    rua: 'Av. Conselheiro Aguiar',
    nivel: 'alto',
    latitude: -8.1214,
    longitude: -34.9006,
    descricao: 'Histórico de alagamentos em dias de chuva intensa'
  },
  {
    id: 2,
    bairro: 'Imbiribeira',
    rua: 'Av. Mascarenhas de Moraes',
    nivel: 'medio',
    latitude: -8.099,
    longitude: -34.914,
    descricao: 'Ponto de acúmulo frequente'
  }
];

router.get('/', (req, res) => {
  res.json(areasRisco);
});

router.get('/:id', (req, res) => {
  const id = Number(req.params.id);
  const area = areasRisco.find(item => item.id === id);

  if (!area) {
    return res.status(404).json({ erro: 'Área de risco não encontrada' });
  }

  res.json(area);
});

module.exports = router;