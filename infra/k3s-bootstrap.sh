#!/usr/bin/env bash

# k3s Bootstrap Script para EnergySuite Local & Produção Platform
# Uso: sudo ./infra/k3s-bootstrap.sh

set -eo pipefail

echo "🚀 Iniciando Bootstrap da Infraestrutura k3s..."

# Configurar kubeconfig
mkdir -p ~/.kube
sudo cp /etc/rancher/k3s/k3s.yaml ~/.kube/config 2>/dev/null || true
sudo chown $(id -u):$(id -g) ~/.kube/config 2>/dev/null || true
export KUBECONFIG=~/.kube/config

# 1. Instalar/Verificar k3s (com Traefik nativo habilitado)
if ! command -v k3s >/dev/null 2>&1; then
    echo "📦 Instalando k3s..."
    curl -sfL https://get.k3s.io | sh -s - --disable=servicelb
else
    echo "✅ k3s já está instalado."
fi

echo "⚡ Verificando estado dos Nós k3s..."
kubectl get nodes || true

# Função para destravar namespaces presos em "Terminating"
clear_stuck_namespace() {
    local ns=$1
    if kubectl get ns "$ns" 2>/dev/null | grep -q "Terminating"; then
        echo "⚠️ Namespace '$ns' está travado em 'Terminating'. Removendo finalizers..."
        python3 -c "
import json, subprocess
try:
    out = subprocess.check_output(['kubectl', 'get', 'namespace', '$ns', '-o', 'json'])
    data = json.loads(out)
    if data.get('status', {}).get('phase') == 'Terminating':
        data['spec']['finalizers'] = []
        p = subprocess.Popen(['kubectl', 'replace', '--raw', '/api/v1/namespaces/$ns/finalize', '-f', '-'], stdin=subprocess.PIPE, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        out, err = p.communicate(input=json.dumps(data).encode())
        print('✅ Finalizers removidos de $ns')
except Exception as e:
    print('Aviso:', e)
" || true
    fi
}

echo "🧹 Verificando namespaces travados..."
clear_stuck_namespace "energysuite"
clear_stuck_namespace "argocd"

# 2. Criar Namespaces essenciais
echo "📁 Criando Namespaces..."
kubectl create namespace energysuite --dry-run=client -o yaml | kubectl apply -f - || true
kubectl create namespace argocd --dry-run=client -o yaml | kubectl apply -f - || true
kubectl create namespace cattle-system --dry-run=client -o yaml | kubectl apply -f - || true

# 3. Instalar Argo CD (usando --server-side para evitar estouro de tamanho de CRD)
echo "🔄 Instalando Argo CD GitOps Controller..."
kubectl apply --server-side -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml || true

# 4. Aplicar Ingress de Infraestrutura (Argo CD & Rancher)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

echo "🛠️ Aplicando Ingress do Argo CD (pc.argocd.com) e Rancher (pc.rancher.com)..."
kubectl apply -f "${PROJECT_ROOT}/infra/k8s/base/infra-services/argocd-ingress.yaml" || true
kubectl apply -f "${PROJECT_ROOT}/infra/k8s/base/infra-services/rancher-ingress.yaml" || true

# 5. Aplicar Manifestos Base Únicos da EnergySuite (energysuite.com & api.energysuite.com)
echo "🛠️ Aplicando Manifestos Unificados K8s (infra/k8s/base)..."
kubectl apply -k "${PROJECT_ROOT}/infra/k8s/base" || true

# 6. Registrar a Aplicação no Argo CD (GitOps CRD)
echo "🔄 Registrando Argo CD Application CRD..."
kubectl apply -f "${PROJECT_ROOT}/infra/k8s/base/gitops/argocd-app.yaml" || true

echo ""
echo "🎉 Bootstrap k3s concluído com sucesso!"
echo "📌 UIs disponíveis conforme sua configuração:"
echo "  - Application: http://energysuite.com / http://api.energysuite.com"
echo "  - Argo CD UI:  http://pc.argocd.com"
echo "  - Rancher UI:  http://pc.rancher.com"
