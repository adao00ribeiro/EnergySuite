#!/usr/bin/env bash

# Deploy Local Script para EnergySuite Kubernetes
# Uso:
#   ./infra/deploy-local.sh            -> Atualiza todos os serviços no Minikube
#   ./infra/deploy-local.sh app-shell  -> Atualiza apenas o app-shell rapidamente

set -eo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "${PROJECT_ROOT}"

SERVICE=${1:-"all"}

echo "🚀 Verificando status do Minikube..."
if ! minikube status >/dev/null 2>&1; then
    echo "❌ Minikube não está rodando! Execute 'minikube start' primeiro."
    exit 1
fi

echo "🚀 Configurando ambiente Docker para o Minikube..."
ENV_OUT=$(minikube -p minikube docker-env)
eval "${ENV_OUT}"

if [ -z "${DOCKER_HOST}" ]; then
    echo "❌ Falha ao vincular o Docker Daemon ao Minikube!"
    exit 1
fi

build_and_deploy() {
    local name=$1
    local path=$2
    echo "📦 [${name}] Compilando imagem diretamente no Minikube..."
    docker build -t energysuite/${name}:latest "${PROJECT_ROOT}/${path}"
    echo "🔄 [${name}] Reiniciando pod no Kubernetes..."
    kubectl rollout restart deployment/${name} -n energysuite
    echo "⏳ [${name}] Aguardando pod ficar ready..."
    kubectl rollout status deployment/${name} -n energysuite --timeout=90s
}

if [ "$SERVICE" = "all" ]; then
    echo "🧹 Removendo Jobs imutáveis e Webhooks travados do NGINX Ingress..."
    kubectl delete job kafka-init-topics -n energysuite --ignore-not-found=true
    kubectl delete validatingwebhookconfiguration ingress-nginx-admission --ignore-not-found=true

    echo "⚡ Aplicando manifestos K8s do overlay DEV..."
    kubectl apply -k "${PROJECT_ROOT}/infra/k8s/overlays/dev"

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

echo "✅ Deploy local concluído com sucesso!"

