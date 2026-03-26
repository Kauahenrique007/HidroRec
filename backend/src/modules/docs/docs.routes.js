const express = require('express');
const asyncHandler = require('../../core/asyncHandler');
const controller = require('./docs.controller');

const router = express.Router();

router.get('/', asyncHandler(controller.getOpenApi));

module.exports = router;
