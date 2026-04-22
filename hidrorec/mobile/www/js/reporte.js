import { initializePage } from './app.js';
import { reportApi } from './api.js';
import { WATER_LEVELS, fileToBase64, showToast } from './utils.js';

initializePage();

const waterGrid = document.getElementById('water-level-grid');
const waterFill = document.getElementById('water-intensity-fill');
const imageInput = document.getElementById('foto');
const imagePreview = document.getElementById('image-preview');
const geoStatus = document.getElementById('geo-status');
const geoCoordinates = document.getElementById('geo-coordinates');
const form = document.getElementById('report-form');
const submitButton = document.getElementById('submit-report');

let selectedWaterLevel = WATER_LEVELS[0].value;
let latitude = -8.0634;
let longitude = -34.8751;

function getFieldValue(id) {
  return document.getElementById(id).value.trim();
}

function renderWaterOptions() {
  waterGrid.innerHTML = WATER_LEVELS.map((item) => `
    <button type="button" class="water-option ${item.value === selectedWaterLevel ? 'is-selected' : ''}" data-level="${item.value}">
      <strong>${item.label}</strong>
      <span class="muted">${item.value === 'Pocas' ? 'Baixa' : item.value}</span>
      <span class="water-option__bar"></span>
    </button>
  `).join('');

  const current = WATER_LEVELS.find((item) => item.value === selectedWaterLevel);
  waterFill.style.width = `${current?.intensity || 20}%`;

  waterGrid.querySelectorAll('[data-level]').forEach((button) => {
    button.addEventListener('click', () => {
      selectedWaterLevel = button.dataset.level;
      renderWaterOptions();
    });
  });
}

function loadGeolocation() {
  if (!navigator.geolocation) {
    geoStatus.textContent = 'GPS indisponivel.';
    return;
  }

  navigator.geolocation.getCurrentPosition((position) => {
    latitude = position.coords.latitude;
    longitude = position.coords.longitude;
    geoStatus.textContent = 'GPS detectado.';
    geoCoordinates.textContent = `${latitude.toFixed(5)}, ${longitude.toFixed(5)}`;
  }, () => {
    geoStatus.textContent = 'GPS nao permitido.';
    geoCoordinates.textContent = 'Usando Recife.';
  });
}

imageInput.addEventListener('change', async () => {
  const [file] = imageInput.files;
  if (!file) {
    imagePreview.innerHTML = '<span class="muted">Nenhuma imagem.</span>';
    return;
  }

  const url = URL.createObjectURL(file);
  imagePreview.innerHTML = `<img src="${url}" alt="Previa da imagem">`;
});

form.addEventListener('submit', async (event) => {
  event.preventDefault();

  if (!form.reportValidity()) {
    return;
  }

  submitButton.disabled = true;
  submitButton.textContent = 'Enviando...';

  try {
    const file = imageInput.files?.[0];
    const payload = {
      titulo: getFieldValue('titulo'),
      descricao: getFieldValue('descricao'),
      nivelAgua: selectedWaterLevel,
      tipoOcorrencia: document.getElementById('tipoOcorrencia').value,
      latitude,
      longitude,
      enderecoReferencia: getFieldValue('enderecoReferencia'),
      bairro: getFieldValue('bairro'),
      regiao: getFieldValue('regiao'),
      nomeUsuario: getFieldValue('nomeUsuario'),
      contatoUsuario: getFieldValue('contatoUsuario'),
      observacoes: getFieldValue('observacoes'),
      fonte: 'Colaborativa',
      imagemBase64: file ? await fileToBase64(file) : null,
      imagemNomeArquivo: file?.name || null,
      imagemContentType: file?.type || null
    };

    await reportApi.create(payload);
    showToast('Reporte enviado.', 'normal');
    form.reset();
    imagePreview.innerHTML = '<span class="muted">Nenhuma imagem.</span>';
    selectedWaterLevel = WATER_LEVELS[0].value;
    renderWaterOptions();
  } catch (error) {
    showToast(error.message, 'alagamento');
  } finally {
    submitButton.disabled = false;
    submitButton.textContent = 'Enviar';
  }
});

renderWaterOptions();
loadGeolocation();
