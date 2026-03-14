// Controller de teste – retorna respostas simples para validar as rotas
exports.getAll = (req, res) => {
  res.json({ mensagem: 'Lista de ocorrências (modo teste)' });
};

exports.getById = (req, res) => {
  res.json({ mensagem: `Detalhes da ocorrência ${req.params.id} (modo teste)` });
};

exports.create = (req, res) => {
  res.status(201).json({ 
    mensagem: 'Ocorrência criada com sucesso (modo teste)', 
    dados: req.body 
  });
};

exports.update = (req, res) => {
  res.json({ 
    mensagem: `Ocorrência ${req.params.id} atualizada (modo teste)`, 
    dados: req.body 
  });
};

exports.delete = (req, res) => {
  res.json({ mensagem: `Ocorrência ${req.params.id} deletada (modo teste)` });
};