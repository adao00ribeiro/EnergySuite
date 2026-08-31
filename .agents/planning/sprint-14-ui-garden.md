# Sprint 14: Garden Design System — Erradicação de Tokens Divergentes e Cores Hardcoded (UI)

**Status:** ✅ **CONCLUÍDA** (2026-08-29) — executada por `frontend-master`; DoD 100% verde (builds + greps).
**Executor:** `frontend-master` (com revisão de padrão pelo `ui-designer`).
**Objetivo:** Zerar os desvios do Design System encontrados na varredura: (1) cada MFE redeclara `:root` com paletas/fontes divergentes do `app-shell`; (2) dezenas de hex/rgba hardcoded e `!important` em SCSS de componentes; (3) light-theme inconsistente; (4) duplicação de animações com durações diferentes; (5) o próprio `ui-designer.md` fora do padrão de estrutura dos demais agentes.

**Justificativa de Negócio:** os 4 MFEs (`mf-portfolio`, `mf-operations`, `mf-hydrology`, `mf-pricing`) hoje carregam Design Systems *paralelos*: tokens próprios (`--bg-base`, `--text-primary`, `--accent-*`) e tipografia `Inter/Outfit`, divergentes do token central `--color-*` (Fira) do `app-shell`. Resultado: mesma tela renderiza diferente conforme o MFE, dark/light quebra ao federar, e o `app-shell` (que deveria ser a referência da regra 5) também tem hex hardcoded grave (`executive-dashboard`, `app-layout`, `counterparty-risk`). Imagem premium/padronizada fica comprometida.

---

## Varredura (fonte da verdade: `.agents/agents/ui-designer.md` + `frontend/app-shell/src/styles.scss`)

### V-1. 🔴 Tokens duplicados por MFE com valores divergentes (viola Regra 3 / Token central)
- `frontend/mf-portfolio/src/styles.scss:2-25` — redeclara `--color-background`, `--color-card`, `--color-muted`, `--color-foreground`, `--color-muted-foreground` em `:root` com valores divergentes (`rgba(30,41,59,0.7)` etc.) e cria `--accent-blue/--accent-rose/--accent-slate/--status-*`.
- `frontend/mf-operations/src/styles.css:2-41` — tokens paralelos `--bg-base`, `--bg-surface`, `--text-primary`, `--accent-indigo/violet/amber/gold`, com variante light própria.
- `frontend/mf-hydrology/src/styles.css:2-24` e `frontend/mf-pricing/src/styles.css:2-30` — mesmo padrão (`--bg-base`/`--text-*`/`--accent-teal/emerald/cyan/blue` / `--accent-cyan/blue/purple/pink`).
- Todos definem `--font-sans: 'Inter'` e `--font-display: 'Outfit'` (viola Design System: Fira Sans / Fira Code). Ex: `mf-portfolio/src/styles.scss:23-24`, `mf-operations/src/styles.css:24-25`.

### V-2. 🔴 Hex colors hardcoded em componentes (viola Regra 2)
- `frontend/app-shell/src/app/features/dashboard/executive-dashboard.component.scss:3-294` — ~25 hex soltos (`#e2e8f0`, `#0f172a`, `#60a5fa`, `#3b82f6`, `#22c55e`, etc.).
- `frontend/app-shell/src/app/layout/app-layout/app-layout.component.scss:6-53` — `#1e1e2d`, `#333333`, `#f5f8fa`, `#69b3ff`.
- `frontend/app-shell/src/app/features/risk/counterparty-risk/counterparty-risk.component.scss:16-56` — `#3f51b5`, `#333`, `#666`, `#888`.
- `frontend/app-shell/src/app/features/users/user-management.component.scss:20,51`, `features/settings/settings-dashboard.component.scss:16,88`, `features/alerts/alerts-dashboard.component.scss:21-63` — fallbacks de Material legado (`--text-secondary,#b0bec5`, `--accent-color,#ff4081`, `--primary-color,#1976d2`).
- `frontend/mf-portfolio/src/app/features/opportunities/components/simulation-dialog/simulation-dialog.component.scss:7-130` — ~15 hex.
- `frontend/mf-portfolio/src/app/features/opportunities/opportunities-book.component.scss:41-59`, `features/strategies/strategies.component.scss:51-86`, `features/portfolio/components/energy-balance-chart/energy-balance-chart.scss:21-66`, `features/dashboard/dashboard.component.scss:47`.
- `frontend/mf-operations/src/app/features/finance/financial-dashboard/financial-dashboard.scss:54-134`, `features/operations/contract-details/contract-details.scss:131`.

### V-3. 🟠 `!important` e `style="..."` (viola Regra 1)
- `frontend/mf-portfolio/src/app/features/strategies/strategies.component.scss:68,86` (`background: rgba(0,0,0,0.4) !important`).
- Aplicar mesmo grep em templates `*.html` dos MFEs por `style=""`.

### V-4. 🟠 Light-theme inconsistente (viola Regra 6)
- Só `mf-operations/src/styles.css:28-41` declara `body.light-theme` — e aponta para tokens locais. `mf-portfolio`, `mf-hydrology`, `mf-pricing` e os componentes do `app-shell` não têm variante light.

### V-5. 🟠 Animações duplicadas com durações divergentes
- Cada MFE reimplementa `.animate-fade-in`/`.animate-slide-up` (`0.4s`) e keyframes próprios (`mf-portfolio/src/styles.scss:58-75`, `mf-operations/src/styles.css:74-91`). `app-shell/src/styles.scss:95-99` usa `0.3s`. O `ui-designer.md` cita "transições de `200ms ease`" — drift entre doc, app-shell e MFEs.

### V-6. 🟠 `ui-designer.md` fora do padrão de estrutura dos agentes
- Demais agentes seguem `# System Prompt` → persona → `## Comportamento Autônomo` → `## Diretrizes Arquiteturais` (lista numerada). O `ui-designer.md` usa seções próprias (`## Design System Oficial`, `## Regras de Padronização`, `## Padrões Reutilizáveis`, `## Verificação ao Finalizar`).
- `tools` do ui-designer inclui `list_dir` que os agentes de dev (`frontend-master`, `backend-architect`) não possuem.
- Não referencia as regras de arquitetura do AGENTS.md (`Frontend_Angular_Master`) que impactam a varredura de UI: `mat-table`, `ReactiveFormsModule`, componentes `standalone` (proíbe NgModules) — o guardião deveria checar isso junto.
- Regra 2 cita exceção `--accent-blue`/`--accent-rose` que só existem no `mf-portfolio` (arquivo violador) e não no token central — referência ambígua vs. os acentos centrais `--color-hydrology/pricing/operations/portfolio/risk` (Regra 3).

---

## PARTE A — TOKENS CENTRAIS & GLOBAIS (`frontend-master`)

### A1. 🔴 UI-1: Remover paletas `:root` duplicadas dos MFEs → referenciar `--color-*` do app-shell
- **Contexto:** V-1. Cada MFE redeclara o design system com nomes/valores próprios (`--bg-base`, `--text-*`, `--accent-*`) e fontes `Inter/Outfit`. Sob Module Federation elas sobrepõem/divergem do `app-shell`.
- **Ação (`frontend-master`):** nos 4 arquivos (`mf-portfolio/src/styles.scss`, `mf-operations/src/styles.css`, `mf-hydrology/src/styles.css`, `mf-pricing/src/styles.css`): descartar a paleta duplicada; manter apenas (a) variáveis *locais e autorizadas* (ex: accent único do módulo — `--color-portfolio` no mf-portfolio) e (b) utilitários próprios que usem `var(--color-*)` centrais. Substituir `--font-sans`/`--font-display` para os tokens do app-shell (Fira) — **remover** `'Inter'`/`'Outfit'`. Se o MFE precisar de accent de dados, usar os tokens centrais da Regra 3/`styles.scss:49-53`.
- **Critérios de Aceite:** `grep -riE "bg-base|text-primary|text-secondary|--accent-|'Inter'|'Outfit'" frontend/mf-*/src/` → 0 hits em estilos globais; `ng build` verde nos 4 MFEs.

### A2. 🔴 UI-2: Mapear hex hardcoded para `var(--color-*)` nos componentes
- **Contexto:** V-2. Mais de 40 ocorrências de hex/rgba soltos, inclusive no próprio `app-shell` (referência deve ser example).
- **Ação (`frontend-master`):** substituir por tokens centrais (`--color-foreground`, `--color-muted-foreground`, `--color-border`, `--color-accent`, `--color-destructive`, etc.). Para estados semânticos (success/warning/info) em telas de dados, usar os acentos do módulo (`--color-*` centrais) ou, quando for dado pontual, declarar o token de exceção **no app-shell central** (`styles.scss`) e referenciar via `var()`. Remover fallbacks hex do padrão `var(--x, #yyyyyy)` — ou padronizar o fallback para token central. Atenção ao `app-shell` que mantém `Material` legado (`--primary-color,#1976d2`, `--text-secondary,#b0bec5`) → migrar para `--color-accent`/`--color-muted-foreground`.
- **Critérios de Aceite:** `grep -rE "#[0-9a-fA-F]{6}" frontend/mf-*/src/app frontend/app-shell/src/app` → 0 (exceção: gradientes declarados com tokens); `ng build` verde em todos os projetos.

### A3. 🟠 UI-3: Remover `!important` e estilos inline
- **Contexto:** V-3. `strategies.component.scss:68,86` usam `!important`; verificar templates por `style="..."`.
- **Ação (`frontend-master`):** eliminar `!important` usando seletores/cascata corretos (encapsulamento de componente ou `:host`). Varrer `*.html` por atributo `style=` e mover para CSS.
- **Critérios de Aceite:** `grep -r "!important" frontend/mf-*/src/app frontend/app-shell/src/app` → 0 (*exceto* os overrides globais centrais de Material em `app-shell/src/styles.scss`); `grep -r 'style="' frontend/mf-*/src/app frontend/app-shell/src/app --include=*.html` → 0.

### A4. 🟠 UI-4: Variantes `body.light-theme` consistentes com o token central
- **Contexto:** V-4. Só o `mf-operations` declara light e com tokens locais.
- **Ação (`frontend-master`):** nos arquivos globais/componentes que referenciam cores contextuais, adicionar/ajustar `body.light-theme` sobre os **mesmos tokens** que o app-shell (`styles.scss:57-73`), nunca com hex fixo novo. Em cada MFE manter apenas a sobrescrita contextual (`--color-card`, `--color-border`, etc.).
- **Critérios de Aceite:** todo componente com cores de contexto possui `body.light-theme` correspondente; grep por hex dentro de blocos `light-theme` dos MFEs → 0.

### A5. 🟠 UI-5: Unificar micro-animações (doc ↔ app-shell ↔ MFEs)
- **Contexto:** V-5. `ui-designer.md` diz `200ms ease`; `app-shell` usa `0.3s`; MFEs usam `0.4s` + keyframes próprios.
- **Ação (`frontend-master`):** padronizar `.animate-fade-in`/`.animate-slide-up` e `@keyframes` numa única definição herdada do `app-shell` (`0.3s`), removendo as duplicadas nos MFEs; garantir `@media (prefers-reduced-motion)` em todos os pontos (UI-designer Regra Acessibilidade).
- **Critérios de Aceite:** uma única definição de `fadeIn/slideUp` por contexto federado; grep por `animation: fadeIn 0.4s` nos MFEs → 0; regra reduced-motion presente nos arquivos que mantêm animações.

---

## PARTE B — DOCUMENTAÇÃO / AGENTE (`frontend-master` em coordenação com `product-owner`)

### B1. 🟠 UI-6: Normalizar `.agents/agents/ui-designer.md` ao padrão dos agentes
- **Contexto:** V-6. Estrutura diverge de `frontend-master.md`/`backend-architect.md`; `list_dir` em `tools`; sem referência às regras do AGENTS.md (`mat-table`, `ReactiveFormsModule`, `standalone`, Signals); exceção `--accent-blue/--accent-rose` ambígua; drift "200ms" vs. real.
- **Ação (`frontend-master`):** alinhar o arquivo:
  1. Padronizar seção de regras como `## Diretrizes Arquiteturais` (lista numerada), mantendo conteúdo.
  2. `tools` alinhado aos agentes de dev (`view_file`, `replace_file_content`, `grep_search`, `run_command`).
  3. Adicionar checagens do AGENTS.md: tabelas `mat-table`, formulários `ReactiveFormsModule`, componentes `standalone: true` (NgModules proibidos), Signals.
  4. Regra 2: substituir a exceção `--accent-blue/--accent-rose` pelos acentos centrais `--color-*` (Regra 3 / `styles.scss:49-53`).
  5. Corrigir timing das animações para o valor real (`0.3s` / `300ms`) sincronizado com `app-shell/src/styles.scss`.
  6. Incluir a verificação de estilos globais dos MFEs (proibição de re-declaração de `:root`/fontes fora do app-shell) na seção de Verificação.
- **Critérios de Aceite:** arquivo segue o mesmo skeleton dos demais agentes; sem referências a `'Inter'`/`'Outfit'`/`200ms` divergentes; regras do AGENTS.md embutidas.

---

## DoD da Sprint
- [x] `ng build` verde em `app-shell`, `mf-portfolio`, `mf-operations`, `mf-hydrology`, `mf-pricing`.
- [x] Grep em estilos de componentes: `#[0-9a-fA-F]{6}` → 0; `!important` → 0 (exceto overrides centrais de Material); `'Inter'`/`'Outfit'` → 0; `--bg-base`/`--text-primary`/`--accent-*` duplicados → 0.
- [x] `body.light-theme` presente e consistente nos arquivos com cores de contexto; sem hex fixo em variantes light.
- [x] Uma única definição de `.animate-fade-in`/`.animate-slide-up`; `prefers-reduced-motion` respeitado.
- [x] `ui-designer.md` validado como fonte revisada e coerente com `styles.scss` (sem drift doc/código).
- [x] Lista de arquivos padronizados reportada pelo executor (padrão das sprints anteriores).

## Fora de Escopo / Sprint 15+
- Substituir mocks de dados restantes da UI (F-1..F-7 do `product-backlog.md` — integração com APIs reais).
- Unificação de `environment.ts`/proxy por MFE (F-7 parcial).
- Auditoria completa de acessibilidade AA (contraste) além da checagem de tokens.