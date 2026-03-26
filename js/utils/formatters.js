import { RISK_META, SEVERITY_META } from './constants.js';

export function formatDateTime(value) {
  if (!value) return '--';
  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short'
  }).format(new Date(value));
}

export function formatNumber(value, digits = 0) {
  return new Intl.NumberFormat('pt-BR', {
    minimumFractionDigits: digits,
    maximumFractionDigits: digits
  }).format(Number(value || 0));
}

export function formatRisk(level) {
  return RISK_META[level]?.label || level || '--';
}

export function formatSeverity(level) {
  return SEVERITY_META[level]?.label || level || '--';
}

export function formatPercentage(value) {
  return `${formatNumber(value, 0)}%`;
}

export function formatList(items = []) {
  return items.filter(Boolean).join(', ');
}
