const express = require('express');
const asyncHandler = require('../../core/asyncHandler');
const controller = require('./territories.controller');

const router = express.Router();

router.get('/', asyncHandler(controller.list));
router.get('/:id', asyncHandler(controller.getById));

module.exports = router;
