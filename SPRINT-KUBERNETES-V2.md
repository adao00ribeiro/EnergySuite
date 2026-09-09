# Sprint Plan v2.0: Readequação da Plataforma Kubernetes (k3s, Traefik, Argo CD & Rancher)

**Projeto**: EnergySuite Platform  
**Especificação de Referência**: [`platform-engineer.md`](file:///media/jarvis/Novo%20volume/AspNetProjects/Energy/EnergySuite/.agents/agents/platform-engineer.md) v2.0  
**Duração estimada**: 3 Sprints (6 dias)  
**Agente responsável**: `Platform_Engineer`  
**Escopo**: Readequação dos manifestos em `infra/k8s/`, remoção do Headlamp, migração para Traefik nativo, implantação declarativa de Argo CD e Rancher, automação de pipeline GitOps e scripts de provisionamento k3s.  
**Status**: ✅ CONCLUÍDA

---

## 📐 Fluxo Arquitetural Target (GitOps)

```text
┌──────────────────────────────┐
│           GitHub             │
│       Código-fonte           │
└──────────────┬───────────────┘
               │
               ▼
        GitHub Actions (Build + Imutável SHA Tag)
               │
               ▼
       Container Registry
               │
               ▼
        GitOps Repository (Manifests K8s)
               │
               ▼
            Argo CD (GitOps Controller)
               │
               ▼
┌──────────────────────────────┐
│       k3s Cluster            │
│                              │
│  ┌────────┐ ┌─────────────┐ │
│  │Rancher │ │ EnergySuite │ │
│  └────────┘ └─────────────┘ │
│                              │
│         Traefik Ingress      │
└──────────────┬───────────────┘
               │
               ▼
   Rede Local LAN (*.home)
```

---

## 🔗 Dependências Globais entre Tasks

```text
[Sprint 1: Core Base K8s & Traefik]
T1.1 (Excluir Headlamp) ─────────┐
T1.2 (Refatorar Ingress Traefik) ┼──► T1.3 (Overlays DEV/PROD) ──► T1.4 (Resource Governance)
                                 │
                                 ▼
                    [Sprint 2: Plataformas Core]
                    T2.1 (Argo CD Application CRD) ──┐
                    T2.2 (Rancher & Argo CD Ingress) ┼──► T2.3 (ConfigMaps & Secrets GitOps)
                                                     │
                                                     ▼
                                        [Sprint 3: CI/CD & Pipeline]
                                        T3.1 (GitHub Actions Tagging SHA) ──┐
                                        T3.2 (Scripts k3s Bootstrap) ──────┼──► T3.3 (Validação DoD)
```

---

## 🏃 SPRINT 1 — Modernização dos Manifestos Base K8s & Migração para Traefik/k3s

### Task 1.1 — Remoção do Headlamp da Arquitetura Base
| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Alta |
| **Arquivos** | `infra/k8s/base/infra-services/headlamp.yaml` [DELETE], `infra/k8s/base/kustomization.yaml`, `infra/k8s/base/ingress.yaml` |
| **Depende de** | Nenhuma |
| **Bloqueia** | T1.2, T1.3 |

**Descrição:** Excluir o arquivo `headlamp.yaml` e remover todas as referências ao Headlamp no `kustomization.yaml` e no `ingress.yaml` base, adequando o projeto à especificação v2.0 que estabeleceu o Rancher como ferramenta única de gestão.

**Critérios de aceite:**
- [x] Arquivo `infra/k8s/base/infra-services/headlamp.yaml` removido do sistema de arquivos.
- [x] Referência a `infra-services/headlamp.yaml` removida do `base/kustomization.yaml`.
- [x] Rota `/headlamp` removida do `base/ingress.yaml`.

---

### Task 1.2 — Refatoração do Ingress Base para Traefik e Hostnames `*.home`
| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Alta |
| **Arquivo** | `infra/k8s/base/ingress.yaml` |
| **Depende de** | T1.1 |
| **Bloqueia** | T1.3, T2.2 |

**Descrição:** Atualizar o `ingress.yaml` base substituindo anotações legadas do NGINX por anotações/especificação padronizadas do Traefik Ingress Controller (padrão k3s) e configurando o host `energysuite.home`.

**Critérios de aceite:**
- [x] Remoção de `kubernetes.io/ingress.class: "nginx"` e substituição por `ingressClassName: traefik` (ou Ingress nativo k3s).
- [x] Adição da regra de Ingress mapeando o host `energysuite.home`.
- [x] Ajuste das rotas dos 5 micro-frontends (`/`, `/mf-hydrology`, `/mf-operations`, `/mf-portfolio`, `/mf-pricing`) e APIs (`/api/v1`, `/api/v1/risk`, `/auth`, `/hubs`).

---

### Task 1.3 — Padronização dos Overlays Kustomize DEV e PROD
| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Média |
| **Arquivos** | `infra/k8s/overlays/dev/ingress.yaml`, `infra/k8s/overlays/dev/kustomization.yaml`, `infra/k8s/overlays/prod/kustomization.yaml` |
| **Depende de** | T1.2 |
| **Bloqueia** | T1.4, T2.1 |

**Descrição:** Adequar os overlays de desenvolvimento e produção ao novo modelo Ingress Traefik e aos nomes de host locais `*.home`.

**Critérios de aceite:**
- [x] `infra/k8s/overlays/dev/ingress.yaml` utiliza o host `energysuite.home`.
- [x] Invocação de `kubectl kustomize infra/k8s/overlays/dev` gera manifestos limpos e sem erros de compilação.

---

### Task 1.4 — Injeção de Resource Governance (Notebook Hardware Constraints)
| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Média |
| **Arquivos** | Deployments em `infra/k8s/base/app-services/*.yaml` e `infra/k8s/base/infra-services/*.yaml` |
| **Depende de** | T1.3 |
| **Bloqueia** | T2.1 |

**Descrição:** Configurar blocos `resources.requests` e `resources.limits` otimizados para restrição de hardware em notebook Debian em todos os Deployments de microserviços e infraestrutura.

**Critérios de aceite:**
- [x] Todos os Deployments possuem `requests` modestos (ex: CPU 50m-100m, Memória 128Mi-256Mi) e `limits` definidos.
- [x] `replicas: 1` explicitamente mantido nos Deployments locais.

---

## 🏃 SPRINT 2 — Implantação Declarativa do Argo CD & Rancher (Plataformas Core)

### Task 2.1 — Manifestos GitOps `ArgoCD Application` CRD
| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Alta |
| **Arquivo** | `infra/k8s/base/gitops/argocd-app.yaml` [NEW] |
| **Depende de** | T1.3, T1.4 |
| **Bloqueia** | T3.1 |

**Descrição:** Criar o recurso declarativo `Application` do Argo CD (`apiVersion: argoproj.io/v1alpha1`) configurado para sincronizar o repositório de GitOps com o cluster k3s.

**Critérios de aceite:**
- [x] Manifesto `argocd-app.yaml` criado declarativamente.
- [x] Configuração de `automated.prune: true` e `automated.selfHeal: true`.
- [x] Apontamento para a pasta de overlay `infra/k8s/overlays/dev`.

---

### Task 2.2 — Ingress para Rancher (`rancher.home`) e Argo CD (`argocd.home`)
| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Média |
| **Arquivos** | `infra/k8s/base/infra-services/rancher-ingress.yaml` [NEW], `infra/k8s/base/infra-services/argocd-ingress.yaml` [NEW] |
| **Depende de** | T1.2 |
| **Bloqueia** | T2.3, T3.3 |

**Descrição:** Criar recursos de Ingress Traefik para expor as interfaces web de gestão do cluster e GitOps nos domínios locais `rancher.home` e `argocd.home`.

**Critérios de aceite:**
- [x] Ingress para Rancher configurado no host `rancher.home`.
- [x] Ingress para Argo CD Server configurado no host `argocd.home`.
- [x] Roteamento HTTP/HTTPS verificado para serviços de gestão do cluster.

---

### Task 2.3 — Centralização de ConfigMaps e Secrets GitOps
| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Média |
| **Arquivos** | `infra/k8s/base/configmaps/env-configmap.yaml`, `infra/k8s/base/secrets/db-secrets.yaml` |
| **Depende de** | T2.2 |
| **Bloqueia** | T3.1 |

**Descrição:** Garantir que o `env-configmap.yaml` injete URLs baseadas em `*.home` para o frontend Angular e microserviços, e validar que credenciais sintéticas fiquem seguras no `db-secrets.yaml`.

**Critérios de aceite:**
- [x] `env-configmap.yaml` com chaves de runtime atualizadas para `energysuite.home`.
- [x] Secrets com desacoplamento de senhas via `secretKeyRef`.

---

## 🏃 SPRINT 3 — Automação CI/CD GitOps, Provisionamento k3s e Validação DoD

### Task 3.1 — Pipeline CI/CD com GitHub Actions e Tagging Imutável
| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Alta |
| **Arquivo** | `.github/workflows/deploy.yml` |
| **Depende de** | T2.1, T2.3 |
| **Bloqueia** | T3.3 |

**Descrição:** Refatorar o workflow do GitHub Actions para remover dependências do Minikube, introduzir o build com tagging imutável (`:${{ github.sha }}`) e automação de atualização de manifestos GitOps.

**Critérios de aceite:**
- [x] Remoção de `minikube docker-env` e de builds `:latest`.
- [x] Imagens Docker geradas com tag imutável derivada do commit SHA.
- [x] Atualização declarativa de tags nos manifestos K8s.

---

### Task 3.2 — Scripts de Bootstrap e Provisionamento k3s
| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Média |
| **Arquivos** | `infra/k3s-bootstrap.sh` [NEW], `infra/deploy-local.sh` |
| **Depende de** | T1.1 |
| **Bloqueia** | T3.3 |

**Descrição:** Criar o script `k3s-bootstrap.sh` para preparar o host Debian com k3s, Traefik, Argo CD e Rancher. Atualizar `deploy-local.sh` para importar imagens diretamente para o containerd do k3s (`k3s ctr images import`).

**Critérios de aceite:**
- [x] `k3s-bootstrap.sh` executável e idêntico a um ambiente declarativo limpo.
- [x] `deploy-local.sh` sem chamadas ao Minikube, com suporte a k3s local.

---

### Task 3.3 — Execução e Validação End-to-End da Definition of Done (v2.0)
| Campo | Valor |
|-------|-------|
| **Agente** | `Platform_Engineer` |
| **Prioridade** | Alta |
| **Arquivos** | Todos em `infra/k8s/` |
| **Depende de** | T3.1, T3.2 |
| **Bloqueia** | Nenhuma |

**Descrição:** Executar testes de integração e validação da Definition of Done completa estabelecida no `platform-engineer.md` v2.0.

**Critérios de aceite:**
- [x] Cluster k3s em status `NodeReady`.
- [x] UIs acessíveis via LAN em `energysuite.home`, `argocd.home` e `rancher.home`.
- [x] Ciclo de GitOps Argo CD verificado e funcional.
- [x] Reconciliação sem intervenções manuais com `kubectl`.

---

## 📊 Matriz de Entregáveis da Readequação v2.0

| # | Task | Artefato Alvo | Status |
|---|------|---------------|--------|
| T1.1 | Remoção do Headlamp | `infra/k8s/base/infra-services/headlamp.yaml` | ✅ Concluído |
| T1.2 | Ingress Traefik Base (`energysuite.home`) | `infra/k8s/base/ingress.yaml` | ✅ Concluído |
| T1.3 | Overlays DEV/PROD para k3s | `infra/k8s/overlays/dev/` | ✅ Concluído |
| T1.4 | Resource Governance (Notebook) | `infra/k8s/base/app-services/*.yaml` | ✅ Concluído |
| T2.1 | Argo CD Application CRD | `infra/k8s/base/gitops/argocd-app.yaml` | ✅ Concluído |
| T2.2 | Ingress Rancher e Argo CD | `infra/k8s/base/infra-services/*-ingress.yaml` | ✅ Concluído |
| T2.3 | ConfigMaps & Secrets GitOps | `infra/k8s/base/configmaps/env-configmap.yaml` | ✅ Concluído |
| T3.1 | Pipeline GitHub Actions SHA Tagging | `.github/workflows/deploy.yml` | ✅ Concluído |
| T3.2 | Script Bootstrap k3s & Local Deploy | `infra/k3s-bootstrap.sh`, `infra/deploy-local.sh` | ✅ Concluído |
| T3.3 | Validação End-to-End DoD v2.0 | Ambientes e Cluster k3s | ✅ Concluído |

---
