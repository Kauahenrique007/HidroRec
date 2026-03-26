const cors = require('cors');
const express = require('express');
const path = require('path');
const env = require('./config/env');
const logger = require('./core/logger');
const requestContext = require('./middlewares/requestContext');
const errorHandler = require('./middlewares/errorHandler');
const notFoundHandler = require('./middlewares/notFoundHandler');
const v1Routes = require('./routes/v1');
const dashboardService = require('./modules/dashboard/dashboard.service');
const incidentsService = require('./modules/incidents/incidents.service');
const monitoringService = require('./modules/monitoring/monitoring.service');
const territoriesService = require('./modules/territories/territories.service');
const alertsService = require('./modules/alerts/alerts.service');

const app = express();

app.disable('x-powered-by');
app.use(cors({ origin: env.corsOrigin }));
app.use(express.json({ limit: '1mb' }));
app.use(express.urlencoded({ extended: true, limit: '1mb' }));
app.use(requestContext);
app.use((req, res, next) => {
  res.setHeader('X-Content-Type-Options', 'nosniff');
  res.setHeader('X-Frame-Options', 'DENY');
  res.setHeader('Referrer-Policy', 'same-origin');
  res.setHeader('Permissions-Policy', 'geolocation=(self)');
  next();
});
app.use((req, res, next) => {
  logger.info('request_received', {
    requestId: res.locals.requestId,
    method: req.method,
    path: req.originalUrl
  });
  next();
});

app.use(express.static(env.publicDir));
app.use('/js', express.static(env.assets.js));
app.use('/style', express.static(env.assets.style));
app.use('/img', express.static(env.assets.img));

app.get('/', (req, res) => {
  res.sendFile(path.join(env.publicDir, 'index.html'));
});

app.get('/api/status', async (req, res, next) => {
  try {
    const monitoring = await monitoringService.getIntegrationStatus();
    res.json({
      status: 'online',
      timestamp: new Date().toISOString(),
      monitoring
    });
  } catch (error) {
    next(error);
  }
});

app.use(`${env.apiPrefix}/${env.apiVersion}`, v1Routes);

app.get('/api/dashboard/resumo', async (req, res, next) => {
  try {
    res.json(await dashboardService.getOverview());
  } catch (error) {
    next(error);
  }
});

app.get('/api/alertas', async (req, res, next) => {
  try {
    const result = await alertsService.listAlerts(req.query);
    res.json(result.data);
  } catch (error) {
    next(error);
  }
});

app.get('/api/areas-risco', async (req, res, next) => {
  try {
    const result = await territoriesService.listTerritories({ ...req.query, page: 1, pageSize: 100 });
    res.json(result.data);
  } catch (error) {
    next(error);
  }
});

app.get('/api/ocorrencias', async (req, res, next) => {
  try {
    const result = await incidentsService.listIncidents(req.query);
    res.json(result.data);
  } catch (error) {
    next(error);
  }
});

app.get('/api/clima', async (req, res, next) => {
  try {
    const result = await monitoringService.getMonitoringOverview({ refresh: false });
    res.json(result.climate);
  } catch (error) {
    next(error);
  }
});

app.get('/api/mare', async (req, res, next) => {
  try {
    const result = await monitoringService.getMonitoringOverview({ refresh: false });
    res.json(result.tide);
  } catch (error) {
    next(error);
  }
});

app.use(notFoundHandler);
app.use(errorHandler);

module.exports = app;
