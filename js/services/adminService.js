import { api } from './api.js';

export const adminService = {
  getOverview() {
    return api.get('/admin/overview');
  },
  listUsers() {
    return api.get('/admin/users');
  },
  listAuditLogs() {
    return api.get('/admin/audit-logs');
  }
};
