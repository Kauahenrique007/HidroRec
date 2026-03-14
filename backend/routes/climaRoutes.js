const express = require('express');
const router = express.Router();

router.get('/', (req, res) => {
  res.json({
    local: 'Recife',
    chuvaMm: 42,
    temperatura: 27,
    umidade: 88,
    fonte: 'mock',
    dataHora: new Date().toISOString()
  });
});

module.exports = router;