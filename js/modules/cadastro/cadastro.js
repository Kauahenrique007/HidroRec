import { initializeShell } from '../../app.js';
import { climateService } from '../../services/climateService.js';
import { login } from '../../services/authService.js';
import { reportsService } from '../../services/reportsService.js';
import { clearFeedback, serializeForm, setButtonBusy, setFeedback, toggleVisibility } from '../../utils/helpers.js';
import { tideService } from '../../services/tideService.js';
import { validateIncidentPayload, validateLoginPayload } from '../../utils/validators.js';

async function mountAuthenticatedView(user) {
  document.getElementById('operator-name').textContent = user.name;
  document.querySelectorAll('[data-session-status]').forEach((target) => {
    target.textContent = `${user.name} - ${user.role}`;
  });
  toggleVisibility(document.getElementById('auth-gate'), false);
  toggleVisibility(document.getElementById('operations-section'), true);

  const reporterField = document.getElementById('reporterName');
  if (reporterField && !reporterField.value) {
    reporterField.value = user.name;
  }
}

function ensureHiddenField(form, name) {
  let field = form.querySelector(`[name="${name}"]`);
  if (field) return field;

  field = document.createElement('input');
  field.type = 'hidden';
  field.name = name;
  form.append(field);
  return field;
}

async function attachGeolocation(form) {
  ensureHiddenField(form, 'latitude');
  ensureHiddenField(form, 'longitude');

  if (!('geolocation' in navigator)) {
    return;
  }

  navigator.geolocation.getCurrentPosition(
    (position) => {
      form.elements.latitude.value = String(position.coords.latitude);
      form.elements.longitude.value = String(position.coords.longitude);
    },
    () => {},
    { enableHighAccuracy: true, timeout: 6000, maximumAge: 300000 }
  );
}

function ensureOpsContextSection() {
  let card = document.getElementById('operations-context-card');
  if (card) return card.querySelector('[data-ops-context]');

  const authGate = document.getElementById('auth-gate');
  card = document.createElement('section');
  card.className = 'info-card';
  card.id = 'operations-context-card';
  card.innerHTML = `
    <div class="section-title">
      <div>
        <span class="section-title__eyebrow">Contexto atual</span>
        <h2>Apoio ao registro operacional</h2>
      </div>
    </div>
    <div class="stack" data-ops-context></div>
  `;

  authGate.insertAdjacentElement('afterend', card);
  return card.querySelector('[data-ops-context]');
}

async function initCadastroPage() {
  let currentUser = await initializeShell('cadastro');
  if (currentUser) {
    mountAuthenticatedView(currentUser);
  }

  const authForm = document.getElementById('login-form');
  const authFeedback = document.getElementById('login-feedback');
  const operationsForm = document.getElementById('operations-form');
  const operationsFeedback = document.getElementById('operations-feedback');
  const authSubmit = authForm.querySelector('button[type="submit"]');
  const operationsSubmit = operationsForm.querySelector('button[type="submit"]');
  const opsContextTarget = ensureOpsContextSection();

  opsContextTarget.innerHTML = '<p>Carregando contexto hidroclimatico para apoiar a decisao operacional...</p>';

  try {
    const [climate, tide] = await Promise.all([
      climateService.getCurrentClimate(),
      tideService.getCurrentTide()
    ]);

    opsContextTarget.innerHTML = `
      <p>Chuva observada: <strong>${climate.observedRainMm} mm</strong> e acumulado de <strong>${climate.accumulatedRain24h} mm</strong> nas ultimas 24h.</p>
      <p>Mare atual em <strong>${tide.levelMeters} m</strong> com influencia <strong>${tide.influence}</strong>.</p>
      <p>Descreva impacto viario, comportamento da drenagem e necessidade de resposta.</p>
    `;
  } catch (error) {
    opsContextTarget.innerHTML = '<p>Contexto hidroclimatico indisponivel no momento. O formulario segue operacional.</p>';
  }

  attachGeolocation(operationsForm);

  authForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    clearFeedback(authFeedback);

    const payload = serializeForm(authForm);
    const errors = validateLoginPayload(payload);
    if (errors.length > 0) {
      setFeedback(authFeedback, {
        type: 'error',
        title: 'Credenciais invalidas',
        message: errors.join(' ')
      });
      return;
    }

    try {
      setButtonBusy(authSubmit, true, {
        defaultLabel: 'Iniciar sessao',
        busyLabel: 'Autenticando...'
      });
      const session = await login(payload);
      currentUser = session.user;
      await mountAuthenticatedView(currentUser);
      attachGeolocation(operationsForm);
      setFeedback(authFeedback, {
        type: 'success',
        title: 'Acesso liberado',
        message: 'Sessao operacional iniciada.'
      });
    } catch (error) {
      setFeedback(authFeedback, {
        type: 'error',
        title: 'Falha de autenticacao',
        message: error.message
      });
    } finally {
      setButtonBusy(authSubmit, false, {
        defaultLabel: 'Iniciar sessao'
      });
    }
  });

  operationsForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    clearFeedback(operationsFeedback);

    const payload = serializeForm(operationsForm);
    const errors = validateIncidentPayload(payload);
    if (errors.length > 0) {
      setFeedback(operationsFeedback, {
        type: 'error',
        title: 'Formulario incompleto',
        message: errors.join(' ')
      });
      return;
    }

    try {
      setButtonBusy(operationsSubmit, true, {
        defaultLabel: 'Salvar ocorrencia',
        busyLabel: 'Salvando...'
      });
      await reportsService.createOperationalReport(payload);
      operationsForm.reset();
      if (currentUser) {
        const reporterField = document.getElementById('reporterName');
        if (reporterField) reporterField.value = currentUser.name;
      }
      attachGeolocation(operationsForm);
      setFeedback(operationsFeedback, {
        type: 'success',
        title: 'Registro operacional salvo',
        message: 'Ocorrencia adicionada com trilha de auditoria.'
      });
    } catch (error) {
      setFeedback(operationsFeedback, {
        type: 'error',
        title: 'Falha ao salvar',
        message: error.message
      });
    } finally {
      setButtonBusy(operationsSubmit, false, {
        defaultLabel: 'Salvar ocorrencia'
      });
    }
  });
}

initCadastroPage();
