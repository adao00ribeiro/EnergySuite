#!/usr/bin/env bash

# Deploy Local Script para EnergySuite Kubernetes
# Uso:
#   ./infra/deploy-local.sh            -> Atualiza todos os serviços no Minikube
#   ./infra/deploy-local.sh app-shell  -> Atualiza apenas o app-shell rapidamente

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "${PROJECT_ROOT}"

SERVICE=${1:-"all"}

echo "🚀 Configurando ambiente Docker para o Minikube..."
eval $(minikube -p minikube docker-env 2>/dev/null || true)

build_and_deploy() {
    local name=$1
    local path=$2
    echo "📦 [${name}] Compilando imagem diretamente no Minikube..."
    docker build -t energysuite/${name}:latest "${PROJECT_ROOT}/${path}"
    echo "🔄 [${name}] Reiniciando pod no Kubernetes..."
    kubectl rollout restart deployment/${name} -n energysuite
}

if [ "$SERVICE" = "all" ]; then
    echo "⚡ Atualizando manifestos K8s..."
    kubectl apply -k "${PROJECT_ROOT}/infra/k8s/overlays/prod"

    build_and_deploy "app-shell" "frontend/app-shell"
    build_and_deploy "mf-hydrology" "frontend/mf-hydrology"
    build_and_deploy "mf-operations" "frontend/mf-operations"
    build_and_deploy "mf-portfolio" "frontend/mf-portfolio"
    build_and_deploy "mf-pricing" "frontend/mf-pricing"
    build_and_deploy "etrm-service" "backend/etrm-service"
    build_and_deploy "risk-service" "backend/risk-service"
else
    case $SERVICE in
        app-shell) build_and_deploy "app-shell" "frontend/app-shell" ;;
        mf-hydrology) build_and_deploy "mf-hydrology" "frontend/mf-hydrology" ;;
        mf-operations) build_and_deploy "mf-operations" "frontend/mf-operations" ;;
        mf-portfolio) build_and_deploy "mf-portfolio" "frontend/mf-portfolio" ;;
        mf-pricing) build_and_deploy "mf-pricing" "frontend/mf-pricing" ;;
        etrm-service) build_and_deploy "etrm-service" "backend/etrm-service" ;;
        risk-service) build_and_deploy "risk-service" "backend/risk-service" ;;
        *)
            echo "❌ Serviço desconhecido: $SERVICE"
            echo "Opções válidas: app-shell, mf-hydrology, mf-operations, mf-portfolio, mf-pricing, etrm-service, risk-service, all"
            exit 1
            ;;
    esac
fi

echo "✅ Deploy local concluído com sucesso!"
