const express = require('express');
const router = express.Router();

const alertas = [
  {
    id: 1,
    titulo: 'Alagamento moderado',
    bairro: 'Boa Viagem',
    nivel: 'medio',
    mensagem: 'Acúmulo de água em vias principais',
    dataHora: new Date().toISOString()
  },
  {
    id: 2,
    titulo: 'Alerta de risco alto',
    bairro: 'Santo Amaro',
    nivel: 'alto',
    mensagem: 'Evite deslocamento na área',
    dataHora: new Date().toISOString()
  }
];

router.get('/', (req, res) => {
  res.json(alertas);
});

router.get('/:id', (req, res) => {
  const id = Number(req.params.id);
  const alerta = alertas.find(item => item.id === id);

  if (!alerta) {
    return res.status(404).json({ erro: 'Alerta não encontrado' });
  }

  res.json(alerta);
});

module.exports = router;