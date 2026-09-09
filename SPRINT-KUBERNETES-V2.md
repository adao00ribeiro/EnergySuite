# Sprint Plan v2.0: Readequação da Plataforma Kubernetes (Paridade Total Local/Produção)

**Projeto**: EnergySuite Platform  
**Especificação de Referência**: [`platform-engineer.md`](file:///media/jarvis/Novo%20volume/AspNetProjects/Energy/EnergySuite/.agents/agents/platform-engineer.md) v2.0  
**Arquitetura**: Unificada (Fonte Única da Verdade — *Dev-Prod Parity*)  
**Agente responsável**: `Platform_Engineer`  
**Escopo**: Eliminação de ambientes divergentes (`dev`/`prod`), unificação em `infra/k8s/base`, remoção do Headlamp, migração para Traefik nativo, implantação declarativa do Argo CD e Rancher.  
**Status**: ✅ CONCLUÍDA

---

## 📐 Fluxo Arquitetural Unificado (GitOps - Paridade Local/Produção)

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
   GitOps Manifests (infra/k8s/base)
               │
               ▼
            Argo CD (GitOps Controller)
               │
               ▼
┌──────────────────────────────┐
│       k3s Cluster            │
│  (Notebook Local = Produção) │
│                              │
│  ┌────────┐ ┌─────────────┐ │
│  │Rancher │ │ EnergySuite │ │
│  └────────┘ └─────────────┘ │
│                              │
│         Traefik Ingress      │
└──────────────┬───────────────┘
               │
               ▼
 Hostnames Definidos (energysuite.com, api.energysuite.com, pc.argocd.com, pc.rancher.com)
```

---

## 🏃 SPRINT 1 — Modernização dos Manifestos Base K8s & Migração para Traefik/k3s

### Task 1.1 — Remoção do Headlamp da Arquitetura Base
- [x] Arquivo `headlamp.yaml` removido.
- [x] Referência removida do `base/kustomization.yaml`.
- [x] Rota `/headlamp` removida do `base/ingress.yaml`.

### Task 1.2 — Refatoração do Ingress Base para Traefik e Hostnames Definidos
- [x] Ingress migrado para `ingressClassName: traefik`.
- [x] Mapeamento dos domínios `energysuite.com`, `api.energysuite.com`, `pc.argocd.com`, `pc.rancher.com`.
- [x] Rotas ajustadas para micro-frontends, Keycloak, APIs e SignalR.

### Task 1.3 — Unificação Kustomize (Paridade Total Local/Produção)
- [x] Removida a pasta `infra/k8s/overlays` e eliminada divergência de código entre `dev` e `prod`.
- [x] Todo a especificação declarativa concentrada na fonte única da verdade: `infra/k8s/base`.

### Task 1.4 — Injeção de Resource Governance
- [x] Limites e solicitações de CPU/Memória configurados para suporte contido no host notebook.
- [x] `replicas: 1` explicitamente padronizado.

---

## 🏃 SPRINT 2 — Implantação Declarativa do Argo CD & Rancher (Plataformas Core)

### Task 2.1 — Manifestos GitOps `ArgoCD Application` CRD
- [x] Criado `infra/k8s/base/gitops/argocd-app.yaml` apontando diretamente para `infra/k8s/base`.
- [x] Política de sincronização automática com `prune: true` e `selfHeal: true`.

### Task 2.2 — Ingress para Rancher e Argo CD
- [x] Criados `rancher-ingress.yaml` (`pc.rancher.com`) e `argocd-ingress.yaml` (`pc.argocd.com`).

### Task 2.3 — Centralização de ConfigMaps e Secrets GitOps
- [x] `env-configmap.yaml` atualizado com as URLs `http://energysuite.com` e `http://api.energysuite.com`.

---

## 🏃 SPRINT 3 — Automação CI/CD GitOps, Provisionamento k3s e Validação DoD

### Task 3.1 — Pipeline CI/CD com GitHub Actions e Tagging Imutável
- [x] Workflow do GitHub Actions refatorado para apontar para `infra/k8s/base`.

### Task 3.2 — Scripts de Bootstrap e Provisionamento Unificado
- [x] Script `infra/k3s-bootstrap.sh` apontando para `infra/k8s/base`.
- [x] Script `infra/deploy-local.sh` apontando para `infra/k8s/base`.

### Task 3.3 — Validação End-to-End da Definition of Done (v2.0)
- [x] Cluster e manifestos 100% integrados e idênticos em qualquer ambiente.

---

## 📊 Matriz de Entregáveis da Readequação v2.0

| # | Task | Artefato Alvo | Status |
|---|------|---------------|--------|
| T1.1 | Remoção do Headlamp | `infra/k8s/base/infra-services/headlamp.yaml` | ✅ Concluído |
| T1.2 | Ingress Traefik Base | `infra/k8s/base/ingress.yaml` | ✅ Concluído |
| T1.3 | Unificação Paridade Local/Prod | `infra/k8s/base` (removido `overlays/`) | ✅ Concluído |
| T1.4 | Resource Governance | `infra/k8s/base/app-services/*.yaml` | ✅ Concluído |
| T2.1 | Argo CD Application CRD | `infra/k8s/base/gitops/argocd-app.yaml` | ✅ Concluído |
| T2.2 | Ingress Rancher e Argo CD | `infra/k8s/base/infra-services/*-ingress.yaml` | ✅ Concluído |
| T2.3 | ConfigMaps & Secrets GitOps | `infra/k8s/base/configmaps/env-configmap.yaml` | ✅ Concluído |
| T3.1 | Pipeline GitHub Actions | `.github/workflows/deploy.yml` | ✅ Concluído |
| T3.2 | Script Bootstrap & Deploy | `infra/k3s-bootstrap.sh`, `infra/deploy-local.sh` | ✅ Concluído |
| T3.3 | Validação End-to-End DoD | Ambientes e Cluster k3s | ✅ Concluído |

---
