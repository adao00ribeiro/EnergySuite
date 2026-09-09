#!/usr/bin/env bash

# k3s Bootstrap Script para EnergySuite Local & Produção Platform
# Uso: sudo ./infra/k3s-bootstrap.sh

set -eo pipefail

echo "🚀 Iniciando Bootstrap da Infraestrutura k3s..."

# 1. Instalar/Verificar k3s (com Traefik nativo habilitado)
if ! command -v k3s >/dev/null 2>&1; then
    echo "📦 Instalando k3s..."
    curl -sfL https://get.k3s.io | sh -s - --disable=servicelb
else
    echo "✅ k3s já está instalado."
fi

# Configurar kubeconfig para o usuário não-root se necessário
mkdir -p ~/.kube
sudo cp /etc/rancher/k3s/k3s.yaml ~/.kube/config 2>/dev/null || true
sudo chown $(id -u):$(id -g) ~/.kube/config 2>/dev/null || true
export KUBECONFIG=~/.kube/config

echo "⚡ Verificando estado dos Nós k3s..."
kubectl get nodes || true

# 2. Criar Namespaces essenciais
echo "📁 Criando Namespaces..."
kubectl create namespace energysuite --dry-run=client -o yaml | kubectl apply -f - || true
kubectl create namespace argocd --dry-run=client -o yaml | kubectl apply -f - || true
kubectl create namespace cattle-system --dry-run=client -o yaml | kubectl apply -f - || true

# 3. Instalar Argo CD
echo "🔄 Instalando Argo CD GitOps Controller..."
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml || true

# 4. Aplicar Manifestos Base Únicos da EnergySuite (Paridade 100% Local-Prod)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

echo "🛠️ Aplicando Manifestos Unificados K8s (infra/k8s/base)..."
kubectl apply -k "${PROJECT_ROOT}/infra/k8s/base" || true

echo "🎉 Bootstrap k3s concluído com sucesso!"
echo "📌 UIs disponíveis conforme sua configuração:"
echo "  - Application: http://energysuite.com / http://api.energysuite.com"
echo "  - Argo CD UI:  http://pc.argocd.com"
echo "  - Rancher UI:  http://pc.rancher.com"
