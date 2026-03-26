export const RISK_META = {
  baixo: { label: 'Baixo', tone: 'low' },
  moderado: { label: 'Moderado', tone: 'moderate' },
  alto: { label: 'Alto', tone: 'high' },
  critico: { label: 'Critico', tone: 'critical' }
};

export const SEVERITY_META = {
  observacao: { label: 'Observacao', tone: 'low' },
  atencao: { label: 'Atencao', tone: 'moderate' },
  severo: { label: 'Severo', tone: 'critical' }
};

export const WATER_LEVEL_OPTIONS = [
  { value: 'tornozelo', label: 'Tornozelo' },
  { value: 'medio', label: 'Joelho / medio' },
  { value: 'alto', label: 'Cintura / alto' }
];

export const INCIDENT_TYPE_OPTIONS = [
  { value: 'alagamento', label: 'Alagamento' },
  { value: 'microalagamento', label: 'Microalagamento' },
  { value: 'drenagem', label: 'Falha de drenagem' },
  { value: 'deslizamento', label: 'Movimento de massa' }
];
