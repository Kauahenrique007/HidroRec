const express = require('express');
const asyncHandler = require('../../core/asyncHandler');
const controller = require('./monitoring.controller');

const router = express.Router();

router.get('/overview', asyncHandler(controller.getOverview));
router.get('/timeline', asyncHandler(controller.getTimeline));
router.get('/integrations', asyncHandler(controller.getIntegrations));

module.exports = router;
