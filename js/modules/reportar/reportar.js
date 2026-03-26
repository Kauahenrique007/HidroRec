import { initializeShell } from '../../app.js';
import { reportsService } from '../../services/reportsService.js';
import { climateService } from '../../services/climateService.js';
import { tideService } from '../../services/tideService.js';
import { clearFeedback, serializeForm, setButtonBusy, setFeedback } from '../../utils/helpers.js';
import { validateIncidentPayload } from '../../utils/validators.js';

async function initReportPage() {
  await initializeShell('reportar');

  const form = document.getElementById('public-report-form');
  const feedback = document.getElementById('form-feedback');
  const climateTarget = document.getElementById('public-context');
  const submitButton = form.querySelector('button[type="submit"]');

  try {
    const [climate, tide] = await Promise.all([
      climateService.getCurrentClimate(),
      tideService.getCurrentTide()
    ]);

    climateTarget.innerHTML = `
      <li>Chuva observada: ${climate.observedRainMm} mm</li>
      <li>Acumulado 24h: ${climate.accumulatedRain24h} mm</li>
      <li>Mare atual: ${tide.levelMeters} m (${tide.influence})</li>
    `;
  } catch (error) {
    climateTarget.innerHTML = '<li>Contexto hidrometeorologico indisponivel no momento.</li>';
  }

  form.addEventListener('submit', async (event) => {
    event.preventDefault();
    clearFeedback(feedback);

    const payload = serializeForm(form);
    const errors = validateIncidentPayload(payload);
    if (errors.length > 0) {
      setFeedback(feedback, {
        type: 'error',
        title: 'Envio bloqueado',
        message: errors.join(' ')
      });
      return;
    }

    try {
      setButtonBusy(submitButton, true, {
        defaultLabel: 'Enviar reporte',
        busyLabel: 'Enviando...'
      });
      await reportsService.createPublicReport(payload);
      form.reset();
      setFeedback(feedback, {
        type: 'success',
        title: 'Reporte registrado',
        message: 'Sua observacao entrou na fila de triagem operacional.'
      });
    } catch (error) {
      setFeedback(feedback, {
        type: 'error',
        title: 'Falha no envio',
        message: error.message
      });
    } finally {
      setButtonBusy(submitButton, false, {
        defaultLabel: 'Enviar reporte'
      });
    }
  });
}

initReportPage();
