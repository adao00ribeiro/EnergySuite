# Sprint: Formulários de Prospecção e Simulação

## Objetivo da Sprint
Implementar os formulários reais (Wizards e Dialogs) para a criação de novos estudos de prospecção (Prospect) e simulações de risco (Pricing). O objetivo é substituir os mocks de UI existentes, fechando o ciclo de ponta a ponta entre o Micro-Frontend Angular (`mf-pricing`) e o Backend CQRS em C# (`etrm-service`).

## Contexto de Negócio
O sistema foi analisado em comparação aos líderes de mercado e a arquitetura de backend já contempla a criação, clonagem e execução de Estudos Prospectivos para projeção de PLD (arquivos oficiais da CCEE) e geração de integrações em MLOps. Esta sprint entrega o componente visual responsável por coletar os parâmetros do usuário para injetá-los no Event Bus via C#.

---

## Task Board (To-Do)

### 1. Backend e API de Integração
- [ ] **Mapeamento de Rotas no MFE:** Garantir que o serviço `ProspectService` (`frontend/mf-pricing/src/app/features/prospect/services/prospect.service.ts`) implemente o método `createStudy(payload)` apontando corretamente para o Gateway ou para a base URL da API local `POST /api/v1/prospect/studies`.
- [ ] **Definição de DTOs no Angular:** Criar a interface Typescript que mapeie a estrutura exata do `CreateStudyCommand` (Name, Description, Model, StartDate, HorizonMonths).

### 2. UI/UX: Novo Estudo (Prospect)
- [ ] **Criar Componente `NewStudyDialogComponent`:**
  - Diretório: `frontend/mf-pricing/src/app/features/prospect/components/new-study-dialog/`
  - Utilizar `@angular/material/dialog`.
- [ ] **Implementar Reactive Forms:**
  - `name`: Text Input, Obrigatório.
  - `description`: Text Area, Opcional.
  - `model`: Dropdown Select (NEWAVE, DECOMP, DESSEM).
  - `startDate`: Angular Material Datepicker.
  - `horizonMonths`: Number Input (ex: 12 meses).
- [ ] **Vinculação de Ação no Dashboard:**
  - Modificar `prospect-dashboard.ts` para que o botão "Novo Estudo" (`onNewStudy()`) efetivamente instancie e exiba o `NewStudyDialogComponent`.
  - Configurar reload da tabela de estudos quando o Modal retornar sucesso.

### 3. UI/UX: Nova Simulação (Pricing)
- [ ] **Definir Regra de Simulação:** Validar se a "Nova Simulação" no painel de Pricing compartilha o mesmo escopo de Prospect ou se necessita de um formulário de Risco independente.
- [ ] **Criar Componente `NewSimulationDialogComponent` (se independente):**
  - Diretório: `frontend/mf-pricing/src/app/features/pricing/components/new-simulation-dialog/`
- [ ] **Vinculação no Pricing Dashboard:**
  - Modificar `pricing-dashboard.ts` para que `onNewSimulation()` engatilhe o dialog correto.

### 4. Testes e Qualidade
- [ ] **Testes de Integração:** Submeter um payload pelo UI e validar se a API responde 200 OK.
- [ ] **Validação Visual:** Garantir que as cores e propriedades css obedeçam as variáveis globais de `--text-primary` e transparências do `glass-panel` e `Dark Theme`.
- [ ] **Tratamento de Erros:** Exibir Snackbar material vermelho caso a API recuse a criação do estudo (Bad Request).
