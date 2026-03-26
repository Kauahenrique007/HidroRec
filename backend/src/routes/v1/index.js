const express = require('express');
const adminRoutes = require('../../modules/admin/admin.routes');
const alertsRoutes = require('../../modules/alerts/alerts.routes');
const authRoutes = require('../../modules/auth/auth.routes');
const dashboardRoutes = require('../../modules/dashboard/dashboard.routes');
const docsRoutes = require('../../modules/docs/docs.routes');
const incidentsRoutes = require('../../modules/incidents/incidents.routes');
const monitoringRoutes = require('../../modules/monitoring/monitoring.routes');
const riskRoutes = require('../../modules/risk/risk.routes');
const territoriesRoutes = require('../../modules/territories/territories.routes');

const router = express.Router();

router.use('/admin', adminRoutes);
router.use('/auth', authRoutes);
router.use('/dashboard', dashboardRoutes);
router.use('/territories', territoriesRoutes);
router.use('/alerts', alertsRoutes);
router.use('/incidents', incidentsRoutes);
router.use('/monitoring', monitoringRoutes);
router.use('/risk', riskRoutes);
router.use('/docs', docsRoutes);

module.exports = router;
