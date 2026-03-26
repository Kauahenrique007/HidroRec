const express = require('express');
const asyncHandler = require('../../core/asyncHandler');
const controller = require('./dashboard.controller');

const router = express.Router();

router.get('/overview', asyncHandler(controller.overview));

module.exports = router;
