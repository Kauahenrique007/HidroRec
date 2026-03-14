const express = require('express');
const router = express.Router();

router.get('/', (req, res) => {
  res.json({
    local: 'Recife',
    nivel: 2.1,
    influencia: 'moderada',
    fonte: 'mock',
    dataHora: new Date().toISOString()
  });
});

module.exports = router;