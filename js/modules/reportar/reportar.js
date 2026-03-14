const reportForm = document.getElementById('report-form');
const feedbackDiv = document.getElementById('feedback');

reportForm.addEventListener('submit', async (e) => {
  e.preventDefault();
  
  const formData = new FormData(reportForm);
  const data = Object.fromEntries(formData.entries());
  
  // Adicionar data/hora atual
  data.dataHora = new Date().toISOString();
  data.origem = 'colaborativa';
  data.status = 'pendente';
  
  // Validação simples
  if (!data.bairro || !data.tipo || !data.descricao) {
    showFeedback('Preencha todos os campos obrigatórios.', 'error');
    return;
  }
  
  try {
    const response = await api.post('/ocorrencias', data);
    showFeedback('Ocorrência registrada com sucesso! Obrigado por colaborar.', 'success');
    reportForm.reset();
  } catch (error) {
    showFeedback('Erro ao enviar. Tente novamente.', 'error');
  }
});

function showFeedback(message, type) {
  feedbackDiv.textContent = message;
  feedbackDiv.className = `feedback ${type}`;
  setTimeout(() => {
    feedbackDiv.textContent = '';
    feedbackDiv.className = 'feedback';
  }, 5000);
}