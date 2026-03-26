const express = require('express');
const asyncHandler = require('../../core/asyncHandler');
const authenticate = require('../../middlewares/authenticate');
const validateRequest = require('../../middlewares/validateRequest');
const { createRateLimiter } = require('../../middlewares/rateLimit');
const controller = require('./auth.controller');
const validators = require('./auth.validators');

const router = express.Router();

router.post(
  '/login',
  createRateLimiter({ windowMs: 60 * 1000, limit: 10 }),
  validateRequest(validators.loginValidator),
  asyncHandler(controller.login)
);
router.get('/me', authenticate, asyncHandler(controller.me));

module.exports = router;
