# Plano de Desenvolvimento: MENZA (Energy Portfolio Management)

## Visão Geral
**Menza** é o cockpit da mesa de negociação de energia. A solução permitirá a traders e gestores de portfólio analisar a posição (comprada, vendida, líquido), identificar oportunidades, traçar estratégias e validar operações contra políticas de risco (Imeris), consolidando o ecossistema EnergySuite.

A replicação MVP focará na visão analítica de posição, simulação de gaps e criação de operações que serão despachadas para o módulo BackOps.

---

## 🎯 Arquitetura de Sprints (MVP - Menza)

### Sprint 1: Fundações, Dashboard e Posição Líquida (P0)
**Objetivo:** Estabelecer o coração analítico do Menza, permitindo a seleção de portfólios e visualização da posição consolidada (Compras vs Vendas vs Líquido).

* **Tarefas:**
  - [x] **Backend:** Criar entidades de Domínio (`Opportunity`, `Strategy`, `Simulation`) no `EtrmService.Domain`.
  - [x] **Backend:** Implementar DTOs e Queries (`GetPortfolioPositionQuery`) para agrupar operações ativas e retornar matriz de Posição Mensal/Anual.
  - [x] **Frontend (`mf-portfolio`):** Implementar o Layout Shell (Cockpit) com seletor rápido de Portfólios e filtros temporais.
  - [x] **Frontend:** Desenvolver os Cards de Indicadores (Volume Comprado, Vendido, Posição Líquida, Resultado Estimado).
  - [x] **Frontend:** Integrar ECharts para exibir o Gráfico de Posição Líquida vs Tempo.

### Sprint 2: Análise Detalhada, Gaps e Heatmap (P0)
**Objetivo:** Permitir "drill-down" na posição e visualização de Gaps de mercado utilizando Heatmaps para facilitar a tomada de decisão visual.

* **Tarefas:**
  - [x] **Backend:** Adicionar filtros e granularidade na Query de Posição (por Submercado, Fonte de Energia, Contraparte).
  - [x] **Backend:** Criar Engine de Gaps para calcular e sinalizar Déficits (Necessidade) e Excedentes (Disponibilidade).
  - [x] **Frontend:** Criar a aba "Posição Detalhada" com uma tabela (Data Grid) expandível.
  - [x] **Frontend:** Implementar o *Heatmap* de Submercados vs Meses para rápida identificação visual de escassez/sobra de energia.

### Sprint 3: Estratégias e Oportunidades Comerciais (P0/P1)
**Objetivo:** Sistematizar a inteligência comercial, transformando os gaps calculados em "Oportunidades" rankeadas baseadas em Estratégias pré-definidas.

* **Tarefas:**
  - [x] **Backend:** Implementar CRUD de `Strategy` (Comprar energia, Vender excedente, Arbitragem).
  - [x] **Backend:** Criar o *Opportunity Engine* para varrer os gaps e sugerir operações viáveis (`OpportunityScore`).
  - [x] **Frontend:** Desenvolver a tela de "Gestão de Estratégias" (Kanban de Status: Draft -> Approved).
  - [x] **Frontend:** Desenvolver o **Book de Oportunidades** (Grid com ranking de score, spread estimado e volume).

### Sprint 4: Simulação de Operações e Copilot (P0/P2)
**Objetivo:** O trader escolhe uma oportunidade ou propõe uma negociação manual. O sistema deve projetar o impacto financeiro e energético ("Antes vs Depois").

* **Tarefas:**
  - [x] **Backend:** Criar Command `SimulateOperationCommand` para projetar a posição virtual sem persistir a operação.
  - [x] **Backend:** (Diferencial) Implementar a camada do *AI Trading Copilot* gerando texto em linguagem natural explicando o impacto simulado da operação.
  - [x] **Frontend:** Criar modal/tela de Simulação (Visão Antes/Depois, variação percentual de risco e exposição).
  - [x] **Frontend:** Tela de Chat/Resumo do Copilot exibindo "Maior Exposição" e "Sugestão de Ação".

### Sprint 5: Validação de Risco e Integração BackOps (P0)
**Objetivo:** Fechar o ciclo. Uma simulação aprovada passa por compliance e é enviada como Operação oficial para o BackOps.

* **Tarefas:**
  - [x] **Backend:** Criar serviço mock `IRiskValidationService` simulando a política do Imeris (Retornando PASS, WARNING, BLOCK).
  - [x] **Backend:** Implementar o fluxo final: `ApproveOpportunityCommand` que transforma a Oportunidade em `Operation` com status `Draft`.
  - [x] **Integração:** Disparar os eventos de mensageria (RabbitMQ/Kafka) para notificar o BackOps que a nova operação nasceu do Menza.
  - [x] **Frontend:** Desenvolver a UX de aprovação e bloqueio visual caso as políticas de risco não passem, além de botão "Enviar para BackOps".

### Sprint 6: Relatórios, Favoritos e Alertas B2B (P1)
**Objetivo:** Funcionalidades de produtividade, auditoria completa de trading e disparo de webhooks corporativos.

* **Tarefas:**
  - [ ] **Backend:** Criar logs de Auditoria para rastreamento completo de qualquer ação no Menza (Login, Simulação, Criação).
  - [ ] **Backend:** Implementar sistema de alertas/webhooks para notificar gaps bruscos ou violação de políticas.
  - [ ] **Frontend:** Exportação nativa de relatórios de Posição e Oportunidades (CSV, PDF).
  - [ ] **Frontend:** Sistema de "Favoritos" (salvar setups de filtros de portfólio no LocalStorage ou Banco).
