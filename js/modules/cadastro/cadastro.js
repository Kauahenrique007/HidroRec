import { initializeShell } from '../../app.js';
import { login } from '../../services/authService.js';
import { reportsService } from '../../services/reportsService.js';
import { clearFeedback, serializeForm, setButtonBusy, setFeedback, toggleVisibility } from '../../utils/helpers.js';
import { validateIncidentPayload, validateLoginPayload } from '../../utils/validators.js';

async function mountAuthenticatedView(user) {
  document.getElementById('operator-name').textContent = user.name;
  document.querySelectorAll('[data-session-status]').forEach((target) => {
    target.textContent = `${user.name} - ${user.role}`;
  });
  toggleVisibility(document.getElementById('auth-gate'), false);
  toggleVisibility(document.getElementById('operations-section'), true);
}

async function initCadastroPage() {
  const user = await initializeShell('cadastro');
  if (user) {
    mountAuthenticatedView(user);
  }

  const authForm = document.getElementById('login-form');
  const authFeedback = document.getElementById('login-feedback');
  const operationsForm = document.getElementById('operations-form');
  const operationsFeedback = document.getElementById('operations-feedback');
  const authSubmit = authForm.querySelector('button[type="submit"]');
  const operationsSubmit = operationsForm.querySelector('button[type="submit"]');

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
      await mountAuthenticatedView(session.user);
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
