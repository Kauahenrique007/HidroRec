const express = require('express');
const router = express.Router();

router.get('/', (req, res) => {
  res.json({
    alertasAtivos: 3,
    ocorrenciasRecentes: 8,
    chuvaAtualMm: 42,
    mareAtualM: 2.1,
    status: 'atenção',
    ultimaAtualizacao: new Date().toISOString()
  });
});

router.get('/resumo', (req, res) => {
  res.json({
    alertasAtivos: 3,
    ocorrenciasRecentes: 8,
    chuvaAtualMm: 42,
    mareAtualM: 2.1,
    bairrosCriticos: ['Boa Viagem', 'Santo Amaro', 'Imbiribeira'],
    ultimaAtualizacao: new Date().toISOString()
  });
});

module.exports = router;