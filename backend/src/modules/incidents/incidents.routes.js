const express = require('express');
const asyncHandler = require('../../core/asyncHandler');
const authenticate = require('../../middlewares/authenticate');
const authorize = require('../../middlewares/authorize');
const validateRequest = require('../../middlewares/validateRequest');
const { createRateLimiter } = require('../../middlewares/rateLimit');
const controller = require('./incidents.controller');
const validators = require('./incidents.validators');

const router = express.Router();

router.get('/', asyncHandler(controller.list));
router.get('/summary', asyncHandler(controller.summary));
router.get('/:id', asyncHandler(controller.getById));
router.post(
  '/public-report',
  createRateLimiter({ windowMs: 60 * 1000, limit: 10 }),
  validateRequest(validators.createPublicIncidentValidator),
  asyncHandler(controller.createPublic)
);
router.post(
  '/operations',
  authenticate,
  authorize(['admin', 'operator']),
  validateRequest(validators.createOperationalIncidentValidator),
  asyncHandler(controller.createOperational)
);
router.patch(
  '/:id/status',
  authenticate,
  authorize(['admin', 'operator']),
  validateRequest(validators.updateIncidentStatusValidator),
  asyncHandler(controller.updateStatus)
);

module.exports = router;
