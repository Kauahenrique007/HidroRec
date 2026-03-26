import { incidentsService } from './incidentsService.js';

export const reportsService = {
  createPublicReport: incidentsService.createPublicReport,
  createOperationalReport: incidentsService.createOperational
};
