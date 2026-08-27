# Sprint de Desenvolvimento: Segurança, Login e RBAC (Keycloak)

Este documento detalha as tarefas (sprint) necessárias para implementar a camada de Segurança (Autenticação e Autorização) transversal da plataforma EnergySuite, utilizando o Keycloak como provedor de Identidade (IAM).

## Objetivo da Sprint
O objetivo central é proteger a plataforma, garantindo que o `app-shell` exija login no Keycloak (Fluxo OIDC/OAuth2), que as rotas dos micro-frontends fiquem restritas, e que todas as requisições enviadas ao Backend `.NET` contenham o token `Authorization: Bearer`.

---

## 1. Configuração do Core de Autenticação (Frontend `app-shell`)
**Objetivo:** Inicializar o `keycloak-angular` no bootstrap da aplicação.

- [ ] **Configuração do Provider:** Editar `frontend/app-shell/src/app/app.config.ts` para injetar o provedor de inicialização do Keycloak, garantindo que ele rode antes das rotas (via `APP_INITIALIZER`). O método `initializeKeycloak` do arquivo `keycloak-init.factory.ts` já existe e deve ser utilizado.
- [ ] **Integração de Silent SSO:** Adicionar o arquivo `silent-check-sso.html` em `frontend/app-shell/src/assets/` para possibilitar a renovação invisível de tokens sem recarregar a aplicação, além de atualizar o `angular.json` para carregar esse asset.

## 2. Proteção de Roteamento (Auth Guard)
**Objetivo:** Impedir o acesso deslogado às áreas sensíveis do portal.

- [ ] **Criar AuthGuard:** Implementar `frontend/app-shell/src/app/core/auth/auth.guard.ts` herdando da classe abstrata `KeycloakAuthGuard`.
- [ ] **Lógica de Roles:** Implementar na Guard a lógica para verificar se o usuário está autenticado e, opcionalmente, verificar se ele tem os papéis (Roles) definidos nas restrições de cada rota (`route.data.roles`).
- [ ] **Proteger Módulos (`app.routes.ts`):** Envolver todas as rotas filhas do `ShellLayoutComponent` (como `/portfolio`, `/pricing`, `/alerts`, `/settings` e `/users`) com o validador `canActivate: [AuthGuard]`.

## 3. Injeção de Headers e Integração de MFEs
**Objetivo:** Assegurar que as requisições API contenham o Token JWT.

- [ ] **Interceptor HTTP do App-Shell:** Garantir que o `KeycloakBearerInterceptor` esteja devidamente provido na hierarquia raiz para que qualquer requisição direcionada para `/api/*` receba automaticamente o token Bearer.
- [ ] **Análise nos MFEs:** Validar se os Micro-frontends (como `mf-pricing`) conseguem herdar este interceptor do shell, ou se necessitam que o token lhes seja fornecido globalmente via Shared State / Events. (A recomendação para Angular Native Federation é o provisionamento do interceptor no App-Shell e o uso de `HttpClient` compartilhado).

## 4. UI/UX: Menu de Usuário e Controle de Sessão
**Objetivo:** Exibir a identidade ativa e permitir finalização segura da sessão.

- [ ] **Header do App Shell:** Modificar o layout superior (navbar) para exibir o nome e/ou avatar do usuário logado (consultando `keycloakService.loadUserProfile()`).
- [ ] **Ação de Logout:** Adicionar um botão de "Sair" que dispare o `keycloakService.logout()` com redirecionamento correto para o login.

## 5. Backend: Políticas e Rate Limiting Associado
**Objetivo:** Ajustar o `EtrmService.API` para validação robusta.

- [ ] **Configuração de CORS:** Revisar `EtrmService.API/Program.cs` certificando-se de que a política `AllowAll` lida bem com cabeçalhos pré-flight (OPTIONS) e tokens Bearer provindos dos MFEs.
- [ ] **Validação End-to-End:** Após as implementações no frontend, validar o recebimento de chamadas nas Controllers através de um token do Keycloak devidamente validado na pipeline do backend.
- [ ] **Garantia de Claims:** Assegurar que os endpoints mais restritos exijam Roles, ex: `[Authorize(Roles = "Trader")]` ou via Policies.

---
**Status da Sprint:** Planejada  
**Impacto Arquitetural:** Crítico (Condiciona o acesso total ao sistema).
