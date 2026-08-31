#!/usr/bin/env bash
#
# Sobe todos os micro-frontends Angular da EnergySuite de uma vez,
# sem instalar nenhuma dependência extra (usa o ng local de cada projeto).
#
# Uso:
#   ./start.sh            # roda app-shell + 4 MFEs
#   ./start.sh --help     # lista as opções
#
# Portas: app-shell 4200, portfolio 4201, operations 4202, pricing 4203, hydrology 4204
#
set -euo pipefail

cd "$(dirname "$0")"

PIDS=()
NAMES=(app-shell mf-portfolio mf-operations mf-pricing mf-hydrology)
PORTS=(4200 4201 4202 4203 4204)

usage() {
  sed -n '2,/^# Portas:/p' "$0" | sed 's/^# \{0,1\}//'
  exit 0
}

for arg in "$@"; do
  case "$arg" in
    -h|--help) usage ;;
    *) echo "Argumento desconhecido: $arg" >&2; usage ;;
  esac
done

log() { printf '\033[1;36m[%-16s]\033[0m %s\n' "$1" "$2"; }

cleanup() {
  echo
  printf '\033[1;33mParando todos os frontends...\033[0m\n'
  kill "${PIDS[@]}" 2>/dev/null || true
  wait 2>/dev/null || true
  exit 0
}
trap cleanup INT TERM EXIT

for i in "${!NAMES[@]}"; do
  name="${NAMES[$i]}"
  port="${PORTS[$i]}"
  log "$name" "starting ng serve --port $port"
  ( cd "$name" && exec npx ng serve --port "$port" ) &
  PIDS+=($!)
done

log "todos" "frontends iniciando (Ctrl+C para parar todos)"
wait
