# Sprint Plan: Padronização e Adequação da Infraestrutura Kubernetes

**Sprint**: Enterprise Infrastructure & Ingress Standardization  
**Duração estimada**: 3 Sprints (6 dias)  
**Agente responsável**: `Platform_Engineer`  
**Escopo**: Manifestos Kubernetes em `infra/k8s/`, Ingress NGINX, Secrets, Overlays Kustomize e Dynamic Runtime Con**Status**: ✅ CONCLUÍDA

---

## Dependências entre Tasks

```
T1 (Converter NodePort -> ClusterIP) ──┐
                                       ├──► T2 (Criar ingress.yaml) ──► T3 (Registrar no kustomization.yaml)
T4 (Desacoplar Senhas -> Secrets) ─────┘

T5 (Runtime env.js em MFEs) ───────────► T6 (CORS & Proxy limits no Ingress)

T7 (Overlay DEV - LAN 192.168.0.180) ──┐
                                       ├──► T9 (Probes e Telemetria)
T8 (Overlay PROD - SSL / Domain) ──────┘
```

---

## Task 1 — Transição de NodePort para ClusterIP nos Microserviços

| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Alta |
| **Arquivos** | `infra/k8s/base/app-services/app-shell.yaml`, `mf-hydrology.yaml`, `mf-operations.yaml`, `mf-portfolio.yaml`, `mf-pricing.yaml`, `infra/k8s/base/infra-services/headlamp.yaml` |
| **Depende de** | Nenhuma |
| **Bloqueia** | T2, T3 |

**Descrição:** Alterar `type: NodePort` para `type: ClusterIP` e remover atribuição de `nodePort` estática. O roteamento HTTP externo será centralizado exclusivamente no Ingress Controller.

**Critérios de aceite:**
- [x] Nenhum serviço de MFE ou ferramenta usa `NodePort` nos manifestos da pasta `base/`
- [x] Todos os serviços usam `type: ClusterIP` e mantêm portas internas padrão (80 ou 4466)
- [x] Renderização `kubectl kustomize` sem erros sintáticos

---

## Task 2 — Ingress Controller Unificado (`ingress.yaml`)

| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Alta |
| **Arquivo** | `infra/k8s/base/ingress.yaml` |
| **Depende de** | T1 |
| **Bloqueia** | T3, T6 |

**Descrição:** Criar o manifesto de Ingress com suporte a NGINX Ingress Controller mapeando rotas `/`, `/mf-hydrology`, `/mf-operations`, `/mf-portfolio`, `/mf-pricing`, `/api/v1/etrm`, `/api/v1/risk`, `/auth`, `/headlamp`.

**Critérios de aceite:**
- [x] Ingress na API `networking.k8s.io/v1`
- [x] Class de ingress configurada para `nginx`
- [x] Mapeamento das rotas dos 5 micro-frontends, Keycloak, APIs e Headlamp
- [x] Backend Services apontam para os nomes e portas `ClusterIP` corretos

---

## Task 3 — Registro no Kustomization Base

| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Alta |
| **Arquivo** | `infra/k8s/base/kustomization.yaml` |
| **Depende de** | T2 |
| **Bloqueia** | T7, T8 |

**Descrição:** Adicionar `ingress.yaml` na lista de `resources` do `kustomization.yaml` base.

**Critérios de aceite:**
- [x] `ingress.yaml` adicionado em `resources`
- [x] Execução de `kubectl kustomize infra/k8s/base` gera o manifesto completo sem falhas

---

## Task 4 — Mover Credenciais e Connections para Secrets

| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Alta |
| **Arquivos** | `infra/k8s/base/app-services/etrm-service.yaml`, `infra/k8s/base/infra-services/keycloak.yaml`, `infra/k8s/base/secrets/db-secrets.yaml` |
| **Depende de** | Nenhuma |
| **Bloqueia** | T7, T8 |

**Descrição:** Mover strings de conexão de banco de dados e senhas administratas hardcoded para referências seguras via `valueFrom.secretKeyRef` apontando para `db-secrets.yaml`.

**Critérios de aceite:**
- [x] Zero senhas em texto plano nos Deployments de `etrm-service.yaml` e `keycloak.yaml`
- [x] `db-secrets.yaml` contendo as chaves `POSTGRES_USER`, `POSTGRES_PASSWORD`, `ETRM_DB_CONNECTION`

---

## Task 5 — Injeção Dinâmica de Variáveis de Runtime nos MFEs

| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Média |
| **Arquivos** | `infra/k8s/base/configmaps/env-configmap.yaml`, `frontend/app-shell/public/assets/env.js` |
| **Depende de** | Nenhuma |
| **Bloqueia** | T6 |

**Descrição:** Configurar injeção de variáveis de ambiente em tempo de execução para os micro-frontends (URLs do Keycloak e Gateway de APIs) sem necessidade de recompilar as imagens Docker Angular.

**Critérios de aceite:**
- [x] ConfigMap expõe chaves dinâmicas de ambiente
- [x] Micro-frontends leem a configuração dinamicamente via `window.__env`

---

## Task 6 — CORS e Proxy Limits no Ingress

| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Média |
| **Arquivo** | `infra/k8s/base/ingress.yaml` |
| **Depende de** | T2, T5 |
| **Bloqueia** | T7, T8 |

**Descrição:** Configurar anotações do NGINX Ingress para liberar CORS entre micro-frontends e Keycloak, e ajustar `proxy-body-size: "50m"` para permitir uploads de relatórios de energia.

**Critérios de aceite:**
- [x] Anotações `nginx.ingress.kubernetes.io/enable-cors: "true"` ativas
- [x] Anotação `nginx.ingress.kubernetes.io/proxy-body-size: "50m"` ativa

---

## Task 7 — Kustomize Overlay DEV (IP LAN)

| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Média |
| **Diretório** | `infra/k8s/overlays/dev/` |
| **Depende de** | T3, T4, T6 |
| **Bloqueia** | T9 |

**Descrição:** Criar a estrutura de overlay para o ambiente de desenvolvimento local (Minikube / IP LAN `192.168.0.180`).

**Critérios de aceite:**
- [x] `infra/k8s/overlays/dev/kustomization.yaml` estendendo `../../base`
- [x] Overrides de hosts Ingress para IP da rede local

---

## Task 8 — Kustomize Overlay PROD (VPS & SSL)

| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Média |
| **Diretório** | `infra/k8s/overlays/prod/` |
| **Depende de** | T3, T4, T6 |
| **Bloqueia** | T9 |

**Descrição:** Criar a estrutura de overlay para ambiente de produção em VPS com suporte a HTTPS e Cert-Manager / Let's Encrypt.

**Critérios de aceite:**
- [x] `infra/k8s/overlays/prod/kustomization.yaml` estendendo `../../base`
- [x] Suporte a anotações de Cert-Manager e TLS secretos

---

## Task 9 — Observabilidade e Health Probes

| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Média |
| **Arquivos** | Deployments em `infra/k8s/base/app-services/` |
| **Depende de** | T7, T8 |
| **Bloqueia** | Nenhuma |

**Descrição:** Garantir que todos os microserviços tenham Liveness e Readiness Probes ativos e anotações do Prometheus para coleta de métricas.

**Critérios de aceite:**
- [x] `livenessProbe` e `readinessProbe` configurados em `etrm-service`, `risk-service` e MFEs
- [x] Anotações `prometheus.io/scrape: "true"` configuradas

---

## Resumo de Entrega

| # | Task | Artefato Alvo | Agente |
|---|------|---------------|--------|
| T1 | Transição NodePort -> ClusterIP | `infra/k8s/base/app-services/*.yaml` | `Platform_Engineer` |
| T2 | Ingress Unificado | `infra/k8s/base/ingress.yaml` | `Platform_Engineer` |
| T3 | Registro no Kustomization Base | `infra/k8s/base/kustomization.yaml` | `Platform_Engineer` |
| T4 | Mover Senhas para Secrets | `infra/k8s/base/secrets/db-secrets.yaml` | `Platform_Engineer` |
| T5 | Injeção Dinâmica env.js | `infra/k8s/base/configmaps/env-configmap.yaml` | `Platform_Engineer` |
| T6 | CORS & Proxy Limits no Ingress | `infra/k8s/base/ingress.yaml` | `Platform_Engineer` |
| T7 | Overlay Kustomize DEV | `infra/k8s/overlays/dev/` | `Platform_Engineer` |
| T8 | Overlay Kustomize PROD | `infra/k8s/overlays/prod/` | `Platform_Engineer` |
| T9 | Health Probes & Telemetria | `infra/k8s/base/app-services/*.yaml` | `Platform_Engineer` |

**Ordem de execução:** (T1 + T4 em paralelo) ➔ T2 ➔ T3 ➔ T5 ➔ T6 ➔ (T7 + T8 em paralelo) ➔ T9
