# HidroRec

**Plataforma privada de inteligência hidroclimática para monitoramento, análise e prevenção de alagamentos em Recife.**

O **HidroRec** é uma solução web orientada a dados reais, criada para transformar informações hidroclimáticas, territoriais e operacionais em **leitura territorial de risco**, **visualização operacional** e **apoio à decisão**.

Diferente de um aplicativo comum de clima, o HidroRec não tem como foco apenas exibir previsão do tempo. Seu objetivo é **centralizar dados dispersos**, **interpretar o risco por área** e **oferecer uma visão operacional clara** para equipes técnicas, gestão pública, defesa civil e organizações que atuam em áreas vulneráveis.

---

## Visão geral

O HidroRec foi idealizado para responder perguntas como:

- Quais áreas de Recife estão com maior risco agora?
- Por que esse risco está elevado?
- Quais bairros exigem mais atenção?
- Quais eventos recentes sustentam esse cenário?
- Que alertas estão ativos?
- Como a situação evoluiu nas últimas horas ou dias?

---

## Problema que o projeto resolve

Recife convive com:

- chuvas intensas;
- vulnerabilidade urbana;
- recorrência de alagamentos;
- necessidade de resposta rápida;
- dados importantes distribuídos em várias fontes diferentes.

Hoje, informações essenciais para leitura do cenário ficam espalhadas entre páginas de previsão, monitoramento, radar, históricos, estações, avisos e canais institucionais de alerta.

O **HidroRec** resolve esse problema criando uma **camada central de inteligência**, onde clima, território e operação deixam de estar isolados e passam a ser tratados de forma integrada.

---

## Propósito

Transformar dados reais de clima, território e ocorrências em **inteligência territorial acionável** para prevenção de alagamentos em Recife.

---

## Objetivo geral

Desenvolver uma **plataforma privada, web e escalável**, com **front-end em HTML, CSS e JavaScript** e **back-end em Node.js**, capaz de:

- integrar dados hidroclimáticos e territoriais reais;
- calcular níveis de risco por área;
- disponibilizar uma visão operacional para monitoramento;
- oferecer análise histórica;
- apoiar a tomada de decisão.

---

## Objetivos específicos

- Integrar fontes reais de dados oficiais e públicos;
- Consolidar dados climáticos, territoriais e operacionais;
- Georreferenciar informações por bairro, região ou área monitorada;
- Classificar o risco por localidade;
- Disponibilizar mapa e dashboard operacional;
- Registrar histórico de eventos e alertas;
- Permitir filtros por período, severidade e localização;
- Oferecer base para relatórios e decisões institucionais;
- Nascer preparado para evoluir para notificações e automações.

---

## O que o HidroRec é

- Plataforma de inteligência territorial;
- Painel operacional privado;
- Sistema orientado a dados reais;
- Produto modular e escalável;
- Solução inicialmente focada em Recife.

## O que o HidroRec não é

- Rede própria de sensores no MVP;
- Aplicativo social para usuários públicos;
- Substituto direto da Defesa Civil;
- Sistema puramente acadêmico ou teórico;
- Ferramenta original de previsão meteorológica.

O diferencial do HidroRec está em **consumir, organizar, interpretar e operacionalizar dados existentes**.

---

## Base factual do projeto

O projeto nasce apoiado em fontes reais já existentes, o que fortalece sua viabilidade técnica.

### Exemplos de base de dados e contexto

- **APAC**: previsão do tempo, radar meteorológico, acumulado de chuvas, monitoramento e boletins;
- **INMET**: dados meteorológicos em tempo real, estações e histórico via BDMEP;
- **Canais institucionais de alerta**: SMS 40199, WhatsApp oficial e sistemas públicos de alerta.

Isso reforça a estratégia de uma arquitetura **API-first**, sem depender de sensores próprios no início.

---

## Público-alvo

### Público principal

- equipes técnicas de monitoramento;
- gestão pública local;
- defesa civil e operação urbana;
- organizações privadas com ativos em áreas vulneráveis;
- gestores que precisam de leitura territorial de risco.

### Público secundário

- universidades e grupos de pesquisa;
- consultorias urbanas e ambientais;
- empresas de infraestrutura, mobilidade, logística e seguros.

---

## Proposta de valor

Em vez de consultar vários sistemas separados, o usuário recebe uma **leitura única, territorial e operacional do risco**.

### Benefícios gerados

- redução do tempo de leitura do cenário;
- menor dispersão de informação;
- maior capacidade de priorização;
- menos dependência de interpretação manual;
- resposta mais rápida diante de situações críticas.

---

## Stack do projeto

### Front-end
- HTML5
- CSS3
- JavaScript
- Consumo de API REST
- Dashboard responsivo
- Mapa interativo
- Componentes visuais para risco, alertas e histórico

### Back-end
- Node.js
- Express.js
- APIs REST
- Integração com fontes externas
- Processamento de dados
- Regras do motor de risco

### Banco de dados
- PostgreSQL
- PostGIS
- Redis

### Infraestrutura
- Docker
- Deploy em nuvem
- Jobs agendados
- Logs
- Monitoramento
- Backup

---

## Arquitetura do sistema

O HidroRec pode ser dividido em 6 camadas:

1. **Camada de ingestão**  
   Recebe dados externos.

2. **Camada de padronização**  
   Normaliza formatos e resolve inconsistências.

3. **Camada geográfica**  
   Relaciona os dados ao território.

4. **Camada analítica**  
   Calcula risco, tendência e criticidade.

5. **Camada de visualização**  
   Exibe o cenário no dashboard e no mapa.

6. **Camada operacional**  
   Registra alertas, histórico, relatórios e decisões.

---

## Fontes de dados

### Dados climáticos
- previsão do tempo;
- chuva observada;
- acumulado de chuva;
- radar meteorológico;
- estações meteorológicas;
- séries históricas;
- avisos meteorológicos.

### Dados territoriais
- bairros;
- regiões político-administrativas;
- zonas de risco;
- áreas críticas;
- camadas geográficas;
- limites territoriais.

### Dados operacionais
- ocorrências históricas;
- registros de atendimento;
- eventos por localidade;
- histórico de alertas;
- mudanças de estágio de risco.

### Dados de saída institucional
- canais de comunicação;
- histórico de disparos;
- status de envio;
- classificação do evento.

---

## Modelo conceitual de funcionamento

O fluxo central do HidroRec segue a lógica:

**coletar → limpar → padronizar → localizar no mapa → cruzar com histórico → calcular risco → exibir → registrar → alertar**

Esse é o coração do projeto.

---

## Estrutura do sistema

### Papel do front-end

O front-end será responsável por:

- autenticação visual;
- navegação entre páginas;
- dashboard executivo;
- mapa operacional;
- cards de risco;
- filtros;
- histórico;
- relatórios visuais;
- interface administrativa.

### Papel do back-end

O back-end será responsável por:

- autenticação;
- integração com fontes externas;
- ingestão de dados;
- normalização;
- georreferenciamento;
- cálculo de risco;
- persistência histórica;
- geração de endpoints;
- auditoria;
- exportações;
- eventos de alerta.

---

## Módulos do produto

### 1. Autenticação e acesso
- login;
- logout;
- recuperação de senha;
- perfis de usuário;
- controle de permissões.

### 2. Dashboard executivo
- visão geral do cenário;
- áreas críticas;
- alertas ativos;
- chuva acumulada;
- resumo do status do sistema.

### 3. Mapa operacional
- visualização de Recife;
- camadas por bairro/região;
- cores por risco;
- filtros;
- detalhes por clique.

### 4. Motor de risco
- classificação automática;
- explicação do risco;
- histórico comparativo;
- tendência.

### 5. Histórico e eventos
- linha do tempo;
- busca por período;
- comparação histórica;
- consulta por área.

### 6. Alertas
- alertas ativos;
- histórico de alertas;
- criticidade;
- status;
- origem e motivo.

### 7. Relatórios
- exportação;
- resumo por período;
- comparação entre bairros;
- indicadores operacionais.

### 8. Administração
- usuários;
- organizações;
- perfis;
- áreas monitoradas;
- parâmetros do motor de risco.

---

## Funcionalidades do MVP

O MVP do HidroRec deve conter:

- login;
- dashboard principal;
- mapa de Recife;
- áreas monitoradas;
- integração com dados reais;
- classificação de risco por área;
- painel de alertas;
- histórico básico;
- filtros por bairro e período;
- administração de usuários.

---

## Fora do MVP

Para evitar excesso de escopo, o MVP **não precisa ter**:

- app mobile público completo;
- IA avançada;
- sensores próprios;
- recomendação automática de ação;
- integração com dezenas de órgãos;
- chat;
- módulo social;
- gamificação.

---

## Requisitos funcionais

O sistema deve:

- permitir autenticação por usuário;
- exibir painel geral com status do cenário;
- atualizar dados periodicamente;
- mostrar risco por área monitorada;
- exibir mapa com camadas e filtros;
- permitir consulta por bairro/região;
- exibir eventos recentes e históricos;
- registrar geração de alertas;
- mostrar motivo do risco;
- permitir exportação de relatórios;
- manter histórico consultável;
- suportar múltiplos perfis de acesso.

---

## Requisitos não funcionais

O sistema deve ser:

- responsivo;
- seguro;
- auditável;
- escalável;
- estável;
- modular;
- observável;
- fácil de manter.

Além disso, precisa ter:

- boa performance no mapa;
- carregamento progressivo;
- logs de erro;
- cache de consultas pesadas;
- versionamento de API;
- separação entre ambiente de desenvolvimento, homologação e produção.

---

## Regras de negócio iniciais

Exemplo de lógica inicial do risco:

- chuva prevista alta aumenta o risco;
- chuva observada recente aumenta o risco;
- recorrência histórica da área aumenta o risco;
- presença em zona vulnerável aumenta o risco;
- múltiplos fatores simultâneos elevam a criticidade;
- ausência de sinais reduz o risco.

### Níveis finais de classificação

- **Baixo**
- **Moderado**
- **Alto**
- **Crítico**

---

## Motor de risco — versão 1

A primeira versão do motor de risco deve ser baseada em **regras claras e auditáveis**, sem depender de IA.

### Exemplo de composição do score

Cada área monitorada recebe pontos por fatores como:

- chuva prevista nas próximas horas;
- chuva acumulada recente;
- histórico de ocorrências;
- presença em área vulnerável;
- intensidade do aviso meteorológico;
- recorrência recente do mesmo local.

### Exemplo de classificação

- **0 a 24** = Baixo  
- **25 a 49** = Moderado  
- **50 a 74** = Alto  
- **75 a 100** = Crítico  

---

## Estrutura recomendada do projeto

```bash
HidroRec/
├── backend/
│   ├── src/
│   │   ├── config/
│   │   ├── controllers/
│   │   ├── routes/
│   │   ├── services/
│   │   ├── models/
│   │   ├── middlewares/
│   │   ├── utils/
│   │   └── app.js
│   ├── server.js
│   ├── package.json
│   └── .env
│
├── frontend/
│   ├── assets/
│   │   ├── css/
│   │   ├── js/
│   │   ├── images/
│   │   └── icons/
│   ├── pages/
│   │   ├── dashboard.html
│   │   ├── mapa.html
│   │   ├── alertas.html
│   │   ├── historico.html
│   │   └── admin.html
│   └── index.html
│
├── docs/
├── README.md
└── .gitignore
