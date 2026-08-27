# Sprint de Desenvolvimento: Módulos de Gestão (App Shell)

Este documento detalha as tarefas (sprint) necessárias para implementar a camada de "Gestão" dentro do `app-shell` da plataforma EnergySuite. Estes módulos são transversais e atendem a todos os micro-frontends.

## 1. Módulo de Alertas (`/alerts`)
**Objetivo:** Centralizar notificações do sistema, cruzamento de limites de risco, status de processamento do Airflow/MLOps e mensagens do Kafka.

- [ ] **UI/UX:** Criar o componente `AlertsDashboardComponent` no `app-shell`.
- [ ] **Integração Real-Time:** Estabelecer uma conexão via SignalR (ou WebSocket) com o backend para receber notificações em tempo real do barramento (Kafka).
- [ ] **Categorização:** Implementar abas para diferenciar alertas de "Sistema", "Risco" e "Operacional".
- [ ] **Ações Base:** Criar botões para "Marcar todos como lidos" e "Limpar histórico".
- [ ] **Roteamento:** Adicionar a rota `{ path: 'alerts', component: AlertsDashboardComponent }` no `app.routes.ts` do `app-shell`.

## 2. Módulo de Configurações (`/settings`)
**Objetivo:** Gerenciar preferências da plataforma, parametrização global e chaves de API (M2M).

- [ ] **UI/UX:** Criar o componente `SettingsDashboardComponent` com Material Tabs para organizar as áreas de configuração.
- [ ] **Tab "Geral":** Configurações de tema forçado (Claro/Escuro), idioma e fuso horário preferencial.
- [ ] **Tab "Integrações":** Painel para geração e revogação de tokens JWT (API Keys) para integrações M2M externas.
- [ ] **Persistência Backend:** Criar/Integrar um serviço no `.NET` para armazenar as preferências no banco de dados (vinculadas ao Tenant/Usuário).
- [ ] **Roteamento:** Adicionar a rota `{ path: 'settings', component: SettingsDashboardComponent }` no `app.routes.ts`.

## 3. Módulo de Usuários e Acessos (`/users`)
**Objetivo:** Administração de RBAC (Role-Based Access Control) integrando com o Keycloak.

- [ ] **UI/UX:** Criar o componente `UserManagementComponent` listando os usuários ativos em uma Material Table.
- [ ] **Integração IAM (Keycloak):** Implementar comunicação backend-to-backend para buscar e gerenciar usuários diretamente do Keycloak (via API de administração).
- [ ] **Gestão de Papéis (Roles):** Formulário ou Modal para atribuir perfis (`Portfolio Manager`, `Trader`, `Risk Analyst`) a um usuário.
- [ ] **Auditoria:** Mostrar a data do "Último Acesso" e "Logs de Sessão" por usuário.
- [ ] **Roteamento:** Adicionar a rota `{ path: 'users', component: UserManagementComponent }` no `app.routes.ts`.

---
**Status da Sprint:** Planejada
**Impacto Arquitetural:** Alto (Módulos Core transversais a todos os produtos).
