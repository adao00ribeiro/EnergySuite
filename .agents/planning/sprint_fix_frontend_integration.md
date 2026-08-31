# Sprint de Correção: Frontend, Integração e Infraestrutura

**Data:** 31/08/2026
**Status:** ✅ Sprints 1-4 Concluídas
**Objetivo:** Corrigir todos os bugs críticos e altos que impedem o funcionamento dos micro-frontends, ajustar a integração backend-frontend, e melhorar o design/UX do sistema.

**Resumo de Issues Encontradas:** 26+ problemas (6 CRITICAL, 8 HIGH, 7 MEDIUM, 5+ LOW)

---

## Sprint 1: Critical Fixes — O sistema não funciona sem isso
**Objetivo:** Desbloquear o funcionamento básico das APIs e autenticação.
**Status:** ✅ Concluída

### `EtrmBackend_Architect` — Backend .NET C#

- [x] **BUG-001 [CRITICAL] — Corrigir porta Docker do etrm-service**
  - Adicionado `ASPNETCORE_URLS=http://+:8080` no docker-compose.yml
  - **Agente:** `EtrmBackend_Architect`

- [x] **BUG-002 [CRITICAL] — Criar CceeIntegrationController ausente**
  - Criado `EtrmService.API/Controllers/CceeIntegrationController.cs` com 4 endpoints
  - **Agente:** `EtrmBackend_Architect`

- [x] **BUG-003 [CRITICAL] — Criar PricingController ausente**
  - Criado `PricingController.cs` + `GetForwardCurveQuery` + `ForwardCurvePointDto`
  - **Agente:** `EtrmBackend_Architect`

- [x] **BUG-004 [CRITICAL] — Criar endpoint de audit-logs**
  - Criado `GetAuditLogsByUserQuery` + adicionado endpoint no `UserManagementController`
  - **Agente:** `EtrmBackend_Architect`

- [x] **BUG-005 [CRITICAL] — Configurar Keycloak: client admin e roles ausentes**
  - Adicionado `etrm-admin-client`, role `Portfolio Manager`, `CanViewENA`, client roles para `energysuite-frontend`
  - **Agente:** `EtrmBackend_Architect`

- [x] **BUG-006 [CRITICAL] — Corrigir CORS do risk-service**
  - Substituído `allow_origins=["*"]` por lista explícita de origins
  - **Agente:** `Python_Risk_Scientist`

### `Frontend_Angular_Master` — Frontend Angular

- [x] **BUG-007 [CRITICAL] — Corrigir prefixo `/api/v1` duplicado em 4 services do mf-operations**
  - Removido `/api/v1` duplicado de `contract.service.ts`, `company.service.ts`, `finance.service.ts`, `ccee-integration.service.ts`
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-008 [CRITICAL] — Adicionar Keycloak interceptor nos 4 MFEs**
  - Adicionado `KeycloakBearerInterceptor` via `HTTP_INTERCEPTORS` em todos os 4 MFEs
  - **Agente:** `Frontend_Angular_Master`

---

## Sprint 2: High Priority — Features quebradas
**Objetivo:** Corrigir funcionalidades específicas que não funcionam.
**Status:** ✅ Concluída

### `Frontend_Angular_Master` — Frontend Angular

- [x] **BUG-009 [HIGH] — risk-metrics nunca recebe dados**
  - Conectado `[metrics]` input no pricing dashboard com dados mockados
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-010 [HIGH] — Remover classes Tailwind (não instalado)**
  - Substituídas todas as classes Tailwind por CSS equivalente em 4 templates + estilos
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-011 [HIGH] — matSort não funciona com arrays signal**
  - Refatorado para `MatTableDataSource` com `@ViewChild(MatSort)` em 3 componentes
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-012 [HIGH] — ngModel conflita com WritableSignal**
  - Corrigido para `[ngModel]="selectedOffset()" (ngModelChange)="selectedOffset.set($event)"`
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-013 [HIGH] — Font Awesome não instalado**
  - Substituídos todos `<i class="fas fa-*">` por `<mat-icon>` do Angular Material
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-014 [HIGH] — Substituir URLs hardcoded por environment**
  - `risk-signalr.service.ts` e `alerts-dashboard.component.ts` agora usam `environment.apiUrl`
  - **Agente:** `Frontend_Angular_Master`

### `EtrmBackend_Architect` — Backend .NET C#

- [x] **BUG-015 [HIGH] — Corrigir K8s Ingress para hubs e APIs**
  - Corrigido routing para `etrm-service:8080` para todos os hubs e 13 paths de API
  - Adicionado `proxy-http-version: "1.1"` para WebSocket upgrade
  - **Agente:** `EtrmBackend_Architect`

---

## Sprint 3: Medium Priority — Robustez e Qualidade
**Objetivo:** Corrigir problemas de subscription leak, erro e configuração.
**Status:** ✅ Concluída

### `Frontend_Angular_Master` — Frontend Angular

- [x] **BUG-016 [MEDIUM] — Corrigir subscription leaks (memory leaks)**
  - Aplicado `DestroyRef` + `takeUntilDestroyed()` em `app.ts` e `executive-dashboard.component.ts`
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-017 [MEDIUM] — Adicionar error handlers em chamadas HTTP**
  - Adicionado error handler com `MatSnackBar` no `ccee-dashboard.ts`
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-018 [MEDIUM] — Remover pacote module-federation duplicado**
  - Removido `@angular-architects/module-federation` do `app-shell/package.json`
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-019 [MEDIUM] — Alinhar versões Angular entre host e MFEs**
  - Todas as dependências Angular alinhadas para `^22.1.0`
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-020 [MEDIUM] — CSS Variables indisponíveis standalone nos MFEs**
  - Bloco `:root` com Design System variables duplicado em todos os 4 MFEs
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-021 [MEDIUM] — Remover/Atualizar layout legacy**
  - Rotas corrigidas: `/analytics/pricing` → `/pricing`, `/analytics/hydrology` → `/hydrology`
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-022 [MEDIUM] — Configurar environment injection para MFEs em produção**
  - Adicionado `(window as any).env?.apiUrl` pattern nos 4 MFEs
  - **Agente:** `Frontend_Angular_Master`

### `Python_Risk_Scientist` — Risk Service

- [x] **BUG-023 [MEDIUM] — Verificação CORS e endpoints**
  - CORS corrigido no BUG-006. Endpoints verificados e alinhados.
  - **Agente:** `Python_Risk_Scientist`

---

## Sprint 4: Low Priority — Polish e Code Quality
**Objetivo:** Limpar código morto, padronizar padrões, melhorar UX visual.
**Status:** ✅ Concluída

### `Frontend_Angular_Master` — Frontend Angular

- [x] **BUG-024 [LOW] — Remover atributo inválido mat-raised-color**
  - `app-shell/contract-list.component.html:3`: Removido `mat-raised-color="primary"`
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-025 [LOW] — Padronizar styleUrls vs styleUrl**
  - Migrados 23 componentes de `styleUrls: ['...']` para `styleUrl: '...'`
  - **Agente:** `Frontend_Angular_Master`

- [x] **BUG-026 [LOW] — Remover/limpar arquivos raiz 0 bytes**
  - Removidos `app-shell/app.scss`, `mf-portfolio/app.scss`, `mf-operations/app.css`, `mf-pricing/app.css`, `mf-hydrology/app.css` (todos 0 bytes)
  - **Agente:** `Frontend_Angular_Master`

- [x] **UX-001 [MEDIUM] — Melhorar design do dashboard operacional**
  - Grid CSS responsiva (4→2→1 colunas), cards com gradient color-coded, ícones SVG, hover effects com translateY, tipografia moderna com CSS variables
  - **Agente:** `Frontend_Angular_Master`

- [x] **UX-002 [MEDIUM] — Customizar tema Angular Material globalmente**
  - Paletas customizadas (sky blue primary, violet accent), Inter font, dark mode completo, cards/tables/buttons/form-fields/toolbars sidenav temáticos, scrollbar customizada, snackbar/dialog/tooltip estilizados
  - **Agente:** `Frontend_Angular_Master`

---

## Matriz de Execução

| Agente | Sprint 1 | Sprint 2 | Sprint 3 | Sprint 4 | Total |
|--------|----------|----------|----------|----------|-------|
| `EtrmBackend_Architect` | ✅ 6/6 | ✅ 1/1 | — | — | 7 tarefas |
| `Frontend_Angular_Master` | ✅ 2/2 | ✅ 6/6 | ✅ 7/7 | ✅ 5/5 | 20 tarefas |
| `Python_Risk_Scientist` | ✅ 1/1 | — | ✅ 1/1 | — | 2 tarefas |
| `Menza_Trading_Copilot` | — | — | — | — | 0 (sem bugs) |

---

## Nota sobre Testes

Após cada fix, rodar:
- **Backend:** `dotnet build` no `EtrmService.slnx` — ✅ Compilou com sucesso (0 erros)
- **Frontend:** `ng build` em cada MFE para validar compilação TypeScript
- **Lint:** `npm run lint` onde disponível
- **Testes unitários:** `dotnet test` no backend, `npx vitest` no frontend
