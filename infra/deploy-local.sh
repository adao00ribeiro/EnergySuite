#!/usr/bin/env bash

# Deploy Local Script para EnergySuite (k3s + Argo CD GitOps)
# Uso:
#   ./infra/deploy-local.sh            -> Atualiza todos os serviços no k3s
#   ./infra/deploy-local.sh app-shell  -> Atualiza apenas o app-shell rapidamente

set -eo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "${PROJECT_ROOT}"

SERVICE=${1:-"all"}
TAG=${2:-"latest"}

echo "🚀 Verificando status do k3s..."
if ! command -v kubectl >/dev/null 2>&1; then
    echo "❌ kubectl não encontrado! Execute './infra/k3s-bootstrap.sh' primeiro."
    exit 1
fi

build_and_deploy() {
    local name=$1
    local path=$2
    echo "📦 [${name}] Compilando imagem Docker energysuite/${name}:${TAG}..."
    docker build -t "energysuite/${name}:${TAG}" "${PROJECT_ROOT}/${path}"
    
    # Se k3s estiver rodando localmente, importa a imagem para o containerd do k3s
    if command -v k3s >/dev/null 2>&1; then
        echo "📥 [${name}] Importando imagem para o containerd do k3s..."
        docker save "energysuite/${name}:${TAG}" | sudo k3s ctr images import - || true
    fi

    echo "🔄 [${name}] Reiniciando deployment no k3s..."
    kubectl rollout restart deployment/${name} -n energysuite || true
}

if [ "$SERVICE" = "all" ]; then
    echo "⚡ Aplicando manifestos K8s do overlay DEV via Kustomize..."
    kubectl apply -k "${PROJECT_ROOT}/infra/k8s/overlays/dev" || true

    build_and_deploy "app-shell" "frontend/app-shell"
    build_and_deploy "mf-hydrology" "frontend/mf-hydrology"
    build_and_deploy "mf-operations" "frontend/mf-operations"
    build_and_deploy "mf-portfolio" "frontend/mf-portfolio"
    build_and_deploy "mf-pricing" "frontend/mf-pricing"
    build_and_deploy "etrm-service" "backend/etrm-service"
    build_and_deploy "risk-service" "backend/risk-service"
    build_and_deploy "keycloak" "infra/keycloak"
else
    case $SERVICE in
        app-shell) build_and_deploy "app-shell" "frontend/app-shell" ;;
        mf-hydrology) build_and_deploy "mf-hydrology" "frontend/mf-hydrology" ;;
        mf-operations) build_and_deploy "mf-operations" "frontend/mf-operations" ;;
        mf-portfolio) build_and_deploy "mf-portfolio" "frontend/mf-portfolio" ;;
        mf-pricing) build_and_deploy "mf-pricing" "frontend/mf-pricing" ;;
        etrm-service) build_and_deploy "etrm-service" "backend/etrm-service" ;;
        risk-service) build_and_deploy "risk-service" "backend/risk-service" ;;
        keycloak) build_and_deploy "keycloak" "infra/keycloak" ;;
        *)
            echo "❌ Serviço desconhecido: $SERVICE"
            echo "Opções válidas: app-shell, mf-hydrology, mf-operations, mf-portfolio, mf-pricing, etrm-service, risk-service, keycloak, all"
            exit 1
            ;;
    esac
fi

echo "✅ Deploy k3s concluído com sucesso!"
