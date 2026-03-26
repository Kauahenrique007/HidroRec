function hasMinLength(value, min) {
  return typeof value === 'string' && value.trim().length >= min;
}

export function validateLoginPayload(payload) {
  const errors = [];
  if (!hasMinLength(payload.email, 5)) errors.push('Informe um e-mail valido.');
  if (!hasMinLength(payload.password, 6)) errors.push('A senha precisa ter ao menos 6 caracteres.');
  return errors;
}

export function validateIncidentPayload(payload) {
  const errors = [];
  if (!hasMinLength(payload.neighborhoodName, 2)) errors.push('Informe o bairro monitorado.');
  if (!hasMinLength(payload.address, 3)) errors.push('Informe um endereco de referencia.');
  if (!hasMinLength(payload.type, 3)) errors.push('Selecione o tipo de evento.');
  if (!hasMinLength(payload.waterLevel, 3)) errors.push('Selecione o nivel da agua.');
  if (!hasMinLength(payload.description, 8)) errors.push('Descreva o contexto com mais detalhe.');
  return errors;
}
