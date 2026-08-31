# Sprint Plan: Tratamento de Erros de Serviço Indisponível

**Sprint:** Error Handling & Resilience
**Duração estimada:** 2 dias
**Agente responsável:** `Frontend_Angular_Master`
**Escopo:** Apenas `app-shell` (host) — MFEs não são alterados
**Status:** ✅ 100% CONCLUÍDA (build validado) — 2026-08-31

---

## Dependências entre tasks

```
T1 (NotificationService) ──┐
                           ├──► T3 (HTTP Interceptor) ──► T4 (Registrar no app.config)
T2 (Estilos Snackbar) ─────┘

T5 (Skeleton) ─────────────► T7 (Rota wildcard)

T6 (ErrorFallback) ────────► T7 (Rota wildcard)
                           ──► T8 (Keycloak factory)
```

---

## Task 1 — NotificationService

| Campo | Valor |
|-------|-------|
| **Agente** | `Frontend_Angular_Master` |
| **Prioridade** | Alta |
| **Arquivo** | `app-shell/src/app/core/services/notification.service.ts` |
| **Depende de** | Nenhuma |
| **Bloqueia** | T3, T4 |

**Descrição:** Criar service singleton que encapsula `MatSnackBar` com métodos `error()`, `warning()`, `success()`, `info()`. Cada método aplica `panelClass` correto e duração configurável. Erros ficam sem auto-dismiss (usuário fecha manualmente).

**Critérios de aceite:**
- [x] `error(msg)` abre snackbar vermelho, sem timeout, com botão "Fechar"
- [x] `warning(msg)` abre amarelo, 6s
- [x] `success(msg)` abre verde, 4s
- [x] `info(msg)` abre azul, 5s
- [x] Posição: `right + top` (padrão existente no `app.ts`)
- [x] Injetável via `providedIn: 'root'`
- [x] Usa `MatSnackBar` do Material (já instalado)

---

## Task 2 — Estilos dos Snackbars

| Campo | Valor |
|-------|-------|
| **Agente** | `Frontend_Angular_Master` |
| **Prioridade** | Alta |
| **Arquivo** | `app-shell/src/styles.scss` |
| **Depende de** | Nenhuma |
| **Bloqueia** | T3, T4 |

**Descrição:** Adicionar classes CSS para os painéis de snackbar que estão faltando. O `.warn-snackbar` já existe. Adicionar `.error-snackbar`, `.success-snackbar`, `.info-snackbar` seguindo o padrão existente.

**Critérios de aceite:**
- [x] `.error-snackbar` → fundo `#DC2626`, texto `#FFFFFF`
- [x] `.success-snackbar` → fundo `#10B981`, texto `#FFFFFF`
- [x] `.info-snackbar` → fundo `#0EA5E9`, texto `#FFFFFF`
- [x] `.warn-snackbar` → já existe, verificar se está completo
- [x] Border-radius herda do `.mat-mdc-snack-bar-container` existente

---

## Task 3 — HTTP Error Interceptor

| Campo | Valor |
|-------|-------|
| **Agente** | `Frontend_Angular_Master` |
| **Prioridade** | Alta |
| **Arquivo** | `app-shell/src/app/core/interceptors/http-error.interceptor.ts` |
| **Depende de** | T1, T2 |
| **Bloqueia** | T4 |

**Descrição:** Interceptor funcional (`HttpInterceptorFn`) que captura `HttpErrorResponse`, mapeia códigos para mensagens PT-BR, exibe toast via `NotificationService` e re-throw o erro.

**Mapeamento de erros:**

| Código | Mensagem |
|--------|----------|
| `0` / `TimeoutError` | "Serviço temporariamente indisponível. Verifique sua conexão." |
| `408` | "Requisição expirada. Tente novamente." |
| `503` | "Serviço em manutenção. Tente novamente em alguns minutos." |
| `504` | "Servidor demorou a responder. Tente novamente." |
| `5xx` (outros) | "Erro interno do servidor." |
| `401` / `403` | Ignorar (Keycloak trata) |

**Critérios de aceite:**
- [x] Erros `0`, `408`, `503`, `504` disparam toast
- [x] `401`/`403` NÃO disparam toast
- [x] Mensagens em PT-BR
- [x] Erro é re-throwado (componentes ainda recebem via `.error()` do subscribe)
- [x] Não duplica toast se o mesmo erro já foi exibido (debounce de 3s por URL)

---

## Task 4 — Registrar Interceptor no app.config.ts

| Campo | Valor |
|-------|-------|
| **Agente** | `Frontend_Angular_Master` |
| **Prioridade** | Alta |
| **Arquivo** | `app-shell/src/app/app.config.ts` |
| **Depende de** | T3 |
| **Bloqueia** | Nenhuma |

**Descrição:** Adicionar `httpErrorInterceptor` ao array de interceptors no `provideHttpClient`.

**Critérios de aceite:**
- [x] `withInterceptors([keycloakBearerInterceptor, httpErrorInterceptor])`
- [x] Import do interceptor adicionado
- [x] Ordem: bearer primeiro, error segundo (para que o token seja anexado antes do catch)

---

## Task 5 — SkeletonComponent

| Campo | Valor |
|-------|-------|
| **Agente** | `Frontend_Angular_Master` |
| **Prioridade** | Média |
| **Arquivo** | `app-shell/src/app/core/components/skeleton/skeleton.component.ts` |
| **Depende de** | Nenhuma |
| **Bloqueia** | T7 |

**Descrição:** Componente standalone que renderiza placeholders animados (shimmer) para estados de loading.

**Inputs:**
- `variant: 'card' | 'table' | 'text' | 'chart'` (padrão: `'card'`)
- `width: string` (padrão: `'100%'`)
- `height: string` (padrão: `'200px'`)
- `lines: number` (padrão: `3`, apenas para variante `text`)

**Critérios de aceite:**
- [x] Animação shimmer com gradient usando `--color-muted`
- [x] 4 variantes visuais distintas
- [x] Responsivo
- [x] Acessível (`role="status"`, `aria-label="Carregando"`)
- [x] Usa design system existente (CSS variables)

---

## Task 6 — ErrorFallbackComponent

| Campo | Valor |
|-------|-------|
| **Agente** | `Frontend_Angular_Master` |
| **Prioridade** | Alta |
| **Arquivo** | `app-shell/src/app/core/components/error-fallback/error-fallback.component.ts` |
| **Depende de** | Nenhuma |
| **Bloqueia** | T7, T8 |

**Descrição:** Componente standalone de página inteira para exibir quando um serviço está indisponível.

**Layout:**
- Ícone central `error_outline` (Material)
- Título: "Serviço Indisponível"
- Mensagem descritiva (via `input()`)
- Código de erro (via `input()`)
- Botão "Tentar Novamente" → emite `output()` `retry`
- Botão "Recarregar Página" → `location.reload()`

**Critérios de aceite:**
- [x] Tela centralizada vertical e horizontalmente
- [x] Ícone com animação pulse sutil
- [x] Botões seguindo design system (`.btn-primary`, `.btn-secondary`)
- [x] Mensagem customizável via `input()`
- [x] Evento `retry` emitido para componente pai
- [x] Tema respeita dark/light

---

## Task 7 — Rota Wildcard `**`

| Campo | Valor |
|-------|-------|
| **Agente** | `Frontend_Angular_Master` |
| **Prioridade** | Média |
| **Arquivo** | `app-shell/src/app/app.routes.ts` |
| **Depende de** | T5, T6 |
| **Bloqueia** | Nenhuma |

**Descrição:** Adicionar rota `**` (wildcard) no final do array de rotas do `ShellLayoutComponent` children, apontando para `ErrorFallbackComponent`.

**Critérios de aceite:**
- [x] Rota `**` é a ÚLTIMA do array
- [x] Lazy-loaded: `loadComponent: () => import(...)`
- [x] Mensagem: "Página não encontrada"

---

## Task 8 — Keycloak Init Error Handling

| Campo | Valor |
|-------|-------|
| **Agente** | `Frontend_Angular_Master` |
| **Prioridade** | Alta |
| **Arquivo** | `app-shell/src/app/core/auth/keycloak-init.factory.ts` |
| **Depende de** | T1, T6 |
| **Bloqueia** | Nenhuma |

**Descrição:** Modificar `initializeKeycloak` para tratar falha de conexão com Keycloak (timeout, DNS, CORS).

**Lógica:**
```
keycloak.init({...})
  .catch(err => {
    // Toast de erro via NotificationService
    // Navegar para ErrorFallbackComponent
    // Mensagem: "Não foi possível conectar ao servidor de autenticação."
  })
```

**Critérios de aceite:**
- [x] Se Keycloak não responder em 15s, exibe toast + fallback
- [x] Usuário vê ErrorFallback com opção de retry
- [x] Retry chama `initializeKeycloak` novamente
- [x] Console.error com detalhes do erro para debug
- [x] Não trava o bootstrap em loop infinito

---

## Resumo de entrega

| # | Artefato | Tipo | Agente |
|---|----------|------|--------|
| 1 | `notification.service.ts` | Service | `Frontend_Angular_Master` |
| 2 | `styles.scss` (snackbars) | Estilo | `Frontend_Angular_Master` |
| 3 | `http-error.interceptor.ts` | Interceptor | `Frontend_Angular_Master` |
| 4 | `app.config.ts` (registro) | Config | `Frontend_Angular_Master` |
| 5 | `skeleton.component.ts` | Componente | `Frontend_Angular_Master` |
| 6 | `error-fallback.component.ts` | Componente | `Frontend_Angular_Master` |
| 7 | `app.routes.ts` (wildcard) | Config | `Frontend_Angular_Master` |
| 8 | `keycloak-init.factory.ts` | Auth | `Frontend_Angular_Master` |

**Ordem de execução:** T1+T2 (paralelo) → T3 → T4 → T5+T6 (paralelo) → T7 → T8

---

## ✅ Status Final — TODAS AS TASKS CONCLUÍDAS

| # | Artefato | Status |
|---|----------|--------|
| 1 | `notification.service.ts` | ✅ |
| 2 | `styles.scss` (snackbars) | ✅ |
| 3 | `http-error.interceptor.ts` | ✅ |
| 4 | `app.config.ts` (registro) | ✅ |
| 5 | `skeleton.component.ts` | ✅ |
| 6 | `error-fallback.component.ts` | ✅ |
| 7 | `app.routes.ts` (wildcard) | ✅ |
| 8 | `keycloak-init.factory.ts` | ✅ |
| 9 | `bootstrap.ts` (fallback DOM de erro) | ✅ |

**Arquivos criados:**
- `app-shell/src/app/core/services/notification.service.ts`
- `app-shell/src/app/core/interceptors/http-error.interceptor.ts`
- `app-shell/src/app/core/components/skeleton/skeleton.component.ts`
- `app-shell/src/app/core/components/error-fallback/error-fallback.component.ts`

**Arquivos modificados:**
- `app-shell/src/styles.scss` (classes `error/warn/success/info-snackbar`)
- `app-shell/src/app/app.config.ts` (registro do interceptor)
- `app-shell/src/app/app.routes.ts` (rota wildcard `**`)
- `app-shell/src/app/core/auth/keycloak-init.factory.ts` (timeout + tratamento de erro)
- `app-shell/src/bootstrap.ts` (fallback de erro no bootstrap)

**Validação:** `ng build` no `app-shell` → exit 0 (warnings de budget SCSS preexistentes, sem relação com as mudanças).

**Nota de implementação (T8):** O fallback visual quando o Keycloak está fora do ar é renderizado pelo `bootstrap.ts` (DOM estático), pois o `APP_INITIALIZER` bloqueia o bootstrap antes que o `ErrorFallbackComponent` (Angular) possa ser montado. O `NotificationService` é injetado via `inject()` dentro do factory, que roda no contexto de injeção.
