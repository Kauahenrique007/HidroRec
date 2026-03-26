const express = require('express');
const asyncHandler = require('../../core/asyncHandler');
const controller = require('./risk.controller');

const router = express.Router();

router.get('/territories', asyncHandler(controller.listRiskSnapshots));

module.exports = router;
