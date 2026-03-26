const express = require('express');
const asyncHandler = require('../../core/asyncHandler');
const authenticate = require('../../middlewares/authenticate');
const authorize = require('../../middlewares/authorize');
const controller = require('./admin.controller');

const router = express.Router();

router.use(authenticate, authorize(['admin']));
router.get('/overview', asyncHandler(controller.overview));
router.get('/users', asyncHandler(controller.listUsers));
router.get('/audit-logs', asyncHandler(controller.listAuditLogs));

module.exports = router;
