const express = require('express');
const router = express.Router();

let ocorrencias = [
  {
    id: 1,
    bairro: 'Boa Viagem',
    rua: 'Av. Conselheiro Aguiar',
    tipo: 'microalagamento',
    nivelAgua: 'medio',
    descricao: 'Água acumulada na via',
    status: 'pendente',
    origem: 'colaborativa',
    dataHora: new Date().toISOString()
  }
];

router.get('/', (req, res) => {
  res.json(ocorrencias);
});

router.get('/:id', (req, res) => {
  const id = Number(req.params.id);
  const ocorrencia = ocorrencias.find(item => item.id === id);

  if (!ocorrencia) {
    return res.status(404).json({ erro: 'Ocorrência não encontrada' });
  }

  res.json(ocorrencia);
});

router.post('/', (req, res) => {
  const { bairro, rua, tipo, nivelAgua, descricao } = req.body;

  if (!bairro || !tipo || !descricao) {
    return res.status(400).json({
      erro: 'bairro, tipo e descricao são obrigatórios'
    });
  }

  const novaOcorrencia = {
    id: ocorrencias.length ? ocorrencias[ocorrencias.length - 1].id + 1 : 1,
    bairro,
    rua: rua || '',
    tipo,
    nivelAgua: nivelAgua || 'medio',
    descricao,
    status: 'pendente',
    origem: 'colaborativa',
    dataHora: new Date().toISOString()
  };

  ocorrencias.push(novaOcorrencia);
  res.status(201).json(novaOcorrencia);
});

module.exports = router;