# Sprint 9: BackOffice e Operacionalização Pluvia

**Objetivo:** Eliminar dados fake dos módulos de BackOffice Financeiro (Operações) e habilitar o consumo de dados reais no módulo de Hidrologia (Pluvia).

## Tasks (Para o `backend-architect` e `frontend-master`)

### 1. BackOffice Financeiro & CCEE (`mf-operations`)
- **Task OP-1: Consumo Real do Dashboard Financeiro**
  - **Contexto:** Os arrays `mockData` e `mockOperations` dão a ilusão de que as liquidações financeiras da Sprint 4 estão prontas.
  - **Ação (`backend-architect`):** Assegurar que a API de Settlement/Billing retorna os agregados corretos de `FinancialSettlementItem`.
  - **Ação (`frontend-master`):** No arquivo `financial-dashboard.ts`, injetar o `FinanceService` e popular os signals `openSettlements` e `operationsToBill` via HTTP GET.
- **Task OP-2 & OP-3: Tickets e Readjustments**
  - **Contexto:** Detalhes de reajustes contratuais e lista de chamados/tickets estão chumbados no código.
  - **Ação (`frontend-master`):** Realizar o bind real dos objetos JSON vindos da API, atualizando os componentes `tickets-list.ts` e `contract-details.ts`.
- **Task OP-4 & OP-5: Fluxo de Contrapartes e Contratos**
  - **Contexto:** A lista de empresas está fake e os "Quick Action Cards" postam mocks para o service.
  - **Ação (`frontend-master`):** Implementar o form real ou capturar o payload dinamicamente nos Quick Action Cards e consultar a API de contrapartes para a tela `company-list.ts`.

### 2. Módulo Pluvia (Hydrology)
- **Task HY-1: Integração de APIs Geoespaciais (Precipitation Map)**
  - **Contexto:** O `precipitation-map.component.ts` roda o método `generateMockPoints()` para colorir o grid pluviométrico.
  - **Ação (`frontend-master`):** Consumir o endpoint real da API Pluvia/Python e preencher os dados de precipitação ENA. Tratamento de *loading* robusto deve ser implementado.
- **Task HY-2: Automação de Exportações**
  - **Contexto:** O `exports-dashboard.ts` tenta baixar arquivos do GUID estático `d290f1ee-6c54-4b01-90e6-d701748f0851`.
  - **Ação (`frontend-master`):** Tornar esse GUID dinâmico, baseado na última execução selecionada pelo usuário.
- **Task HY-3: Controle de Acesso Baseado em IAM**
  - **Contexto:** `auth.service.ts` em `mf-hydrology` mocka as roles (Claims) simulando o Keycloak.
  - **Ação (`frontend-master`):** Integrar com o Keycloak via `angular-oauth2-oidc` ou repassar o token do App-Shell para o MFE obter as permissões reais.
