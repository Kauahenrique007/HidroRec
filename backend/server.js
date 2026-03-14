const express = require('express');
const cors = require('cors');
const path = require('path');
require('dotenv').config();

// Importar rotas
const dashboardRoutes = require('./routes/dashboardRoutes');
const alertasRoutes = require('./routes/alertasRoutes');
const ocorrenciasRoutes = require('./routes/ocorrenciasRoutes');
const climaRoutes = require('./routes/climaRoutes');
const mareRoutes = require('./routes/mareRoutes');
const areasRiscoRoutes = require('./routes/areasRiscoRoutes');

const app = express();
const PORT = process.env.PORT || 3000;

// Middlewares
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Servir arquivos estáticos do frontend
app.use(express.static(path.join(__dirname, '../frontend')));

// Rotas da API
app.use('/api/dashboard', dashboardRoutes);
app.use('/api/alertas', alertasRoutes);
app.use('/api/ocorrencias', ocorrenciasRoutes);
app.use('/api/clima', climaRoutes);
app.use('/api/mare', mareRoutes);
app.use('/api/areas-risco', areasRiscoRoutes);

// Rota principal
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, '../frontend/index.html'));
});

// Rota de status
app.get('/api/status', (req, res) => {
  res.json({
    status: 'online',
    timestamp: new Date().toISOString()
  });
});

// Middleware para rota não encontrada
app.use((req, res) => {
  res.status(404).json({ erro: 'Rota não encontrada' });
});

// Middleware de erro
app.use((err, req, res, next) => {
  console.error(err.stack);
  res.status(500).json({ erro: 'Erro interno no servidor' });
});

app.listen(PORT, () => {
  console.log(`Servidor rodando na porta ${PORT}`);
});