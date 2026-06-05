#!/usr/bin/env bash
# =============================================================================
# VibraHeka — Terraform Operational Entrypoint
# Compatible: macOS / zsh · bash 3+
# =============================================================================
#
# Uso básico:
#   ./terraform.sh                                      → init + apply
#   ./terraform.sh --workspace <nombre>                 → init + workspace + apply
#   ./terraform.sh -w <nombre>                          → alias de --workspace
#   ./terraform.sh --workspace <nombre> --destroy       → init + workspace + destroy
#   ./terraform.sh --build-lambdas --workspace <nombre> → build + workspace + apply
#   ./terraform.sh --destroy                            → init + destroy (workspace actual)
#   ./terraform.sh --build-lambdas                      → build lambdas + init + apply
#   ./terraform.sh --build-lambdas --destroy            → build lambdas + init + destroy
#   ./terraform.sh --help                               → muestra esta ayuda
#
# ─── MODO SEGURO — por defecto, recomendado ───────────────────────────────────
#   Exporta las variables de entorno ANTES de ejecutar el script.
#   USE_LOCAL_SECRET_OVERRIDES debe permanecer en false (valor por defecto).
#
#   export TF_VAR_stripe_api_key="sk_live_..."
#   export TF_VAR_stripe_webhook_secret="whsec_..."
#   export TF_VAR_password_reset_token_secret="super-secret"
#   export TF_VAR_stripe_event_bus_arn="arn:aws:events:..."
#   ./terraform.sh --workspace dev
#
# ─── MODO LOCAL OVERRIDE — solo desarrollo local ──────────────────────────────
#   1. Edita el bloque "LOCAL SECRET OVERRIDES" más abajo con tus valores reales.
#   2. Cambia USE_LOCAL_SECRET_OVERRIDES=true.
#   3. Ejecuta: ./terraform.sh --workspace dev
#
#   ⚠️  ANTES DE HACER COMMIT:
#       - Restaura USE_LOCAL_SECRET_OVERRIDES=false
#       - Restaura todos los _LOCAL_* a PLACEHOLDER_*
#       - Verifica con: git diff src/Infrastructure/terraform/terraform.sh
#
# ⚠️  NUNCA commitees este fichero con secretos reales. Está versionado.
# =============================================================================

set -euo pipefail

# =============================================================================
# 🔧 LOCAL SECRET OVERRIDES — Hueco controlado para secretos de desarrollo local
# =============================================================================
#
# INSTRUCCIONES:
#   1. Cambia USE_LOCAL_SECRET_OVERRIDES a true  (solo en tu máquina local).
#   2. Sustituye los PLACEHOLDER_* por tus valores reales de desarrollo.
#   3. Ejecuta el script normalmente.
#   4. ANTES DE COMMITEAR: vuelve a false y restaura los PLACEHOLDER_*.
#
# ⚠️  SEGURIDAD: este bloque NO debe contener valores reales en el repositorio.
#     Si tienes dudas, usa el MODO SEGURO con variables de entorno TF_VAR_*.
# =============================================================================
USE_LOCAL_SECRET_OVERRIDES=true   # ← Cambiar a true SOLO en local; nunca commitear en true

_LOCAL_STRIPE_API_KEY=""
_LOCAL_PASSWORD_RESET_TOKEN_SECRET=""
_LOCAL_STRIPE_EVENT_BUS_ARN=""
# =============================================================================

# ─────────────────────────────────────────────────────────────────────────────
# RESOLUCIÓN DE SECRETOS
#   USE_LOCAL_SECRET_OVERRIDES=true  → usa el bloque LOCAL SECRET OVERRIDES
#   USE_LOCAL_SECRET_OVERRIDES=false → usa TF_VAR_* del entorno (o placeholder)
# ─────────────────────────────────────────────────────────────────────────────
if [[ "$USE_LOCAL_SECRET_OVERRIDES" == "true" ]]; then
  export TF_VAR_stripe_api_key="$_LOCAL_STRIPE_API_KEY"
  export TF_VAR_password_reset_token_secret="$_LOCAL_PASSWORD_RESET_TOKEN_SECRET"
  export TF_VAR_stripe_event_bus_arn="$_LOCAL_STRIPE_EVENT_BUS_ARN"
else
  : "${TF_VAR_stripe_api_key:=PLACEHOLDER_STRIPE_API_KEY}"
  : "${TF_VAR_password_reset_token_secret:=PLACEHOLDER_PASSWORD_RESET_TOKEN_SECRET}"
  : "${TF_VAR_stripe_event_bus_arn:=PLACEHOLDER_STRIPE_EVENT_BUS_ARN}"
  export TF_VAR_stripe_api_key
  export TF_VAR_stripe_webhook_secret
  export TF_VAR_stripe_event_bus_arn
fi
# ─────────────────────────────────────────────────────────────────────────────

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LAMBDA_PAYMENTS_DIR="$SCRIPT_DIR/Lambdas/Payments"
LAMBDA_SEND_EMAIL_DIR="$SCRIPT_DIR/Lambdas/SendEmail"
LAMBDA_VERIFICATION_CODE_DIR="$SCRIPT_DIR/Lambdas/VerificationCode"

FLAG_DESTROY=false
FLAG_BUILD_LAMBDAS=false
TARGET_WORKSPACE=""

# ─── Colores y utilidades de UX ──────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
BOLD='\033[1m'
RESET='\033[0m'

info()    { echo -e "${CYAN}ℹ️  $*${RESET}"; }
ok()      { echo -e "${GREEN}✅  $*${RESET}"; }
warn()    { echo -e "${YELLOW}⚠️  $*${RESET}"; }
error()   { echo -e "${RED}❌  $*${RESET}" >&2; }
section() { echo -e "\n${BOLD}${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}\n${BOLD}${BLUE}   $*${RESET}\n${BOLD}${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${RESET}\n"; }

usage() {
  echo -e "${BOLD}Uso:${RESET}"
  echo -e "  ${CYAN}./terraform.sh${RESET}                                       → init + apply"
  echo -e "  ${CYAN}./terraform.sh --workspace <nombre>${RESET}                  → init + workspace + apply"
  echo -e "  ${CYAN}./terraform.sh -w <nombre>${RESET}                           → alias de --workspace"
  echo -e "  ${CYAN}./terraform.sh --workspace <nombre> --destroy${RESET}        → init + workspace + destroy"
  echo -e "  ${CYAN}./terraform.sh --build-lambdas --workspace <nombre>${RESET}  → build + workspace + apply"
  echo -e "  ${CYAN}./terraform.sh --destroy${RESET}                             → init + destroy (workspace actual)"
  echo -e "  ${CYAN}./terraform.sh --build-lambdas${RESET}                       → build lambdas + init + apply"
  echo -e "  ${CYAN}./terraform.sh --build-lambdas --destroy${RESET}             → build lambdas + init + destroy"
  echo -e "  ${CYAN}./terraform.sh --help${RESET}                                → muestra esta ayuda"
  echo ""
  echo -e "${BOLD}Modos de configuración de secretos:${RESET}"
  echo -e "  ${GREEN}[Modo seguro — recomendado]${RESET}  Exporta las variables antes de ejecutar:"
  echo -e "    ${YELLOW}export TF_VAR_stripe_api_key=\"sk_live_...\"${RESET}"
  echo -e "    ${YELLOW}export TF_VAR_password_reset_token_secret=\"super-secret\"${RESET}"
  echo -e "    ${YELLOW}export TF_VAR_stripe_event_bus_arn=\"arn:aws:events:...\"${RESET}"
  echo ""
  echo -e "  ${YELLOW}[Modo local override]${RESET}  Edita el bloque LOCAL SECRET OVERRIDES en terraform.sh"
  echo -e "    y cambia ${BOLD}USE_LOCAL_SECRET_OVERRIDES=true${RESET}."
  echo -e "    ${RED}⚠️  NUNCA commitees ese fichero con valores reales.${RESET}"
}

# ─── Parseo de flags ─────────────────────────────────────────────────────────
while [[ $# -gt 0 ]]; do
  case "$1" in
    --destroy)
      FLAG_DESTROY=true
      shift
      ;;
    --build-lambdas)
      FLAG_BUILD_LAMBDAS=true
      shift
      ;;
    --workspace|-w)
      if [[ $# -lt 2 ]] || [[ -z "${2:-}" ]] || [[ "$2" == -* ]]; then
        error "El flag '$1' requiere un valor (nombre del workspace). Ejemplo: $1 dev"
        echo ""
        usage
        exit 1
      fi
      TARGET_WORKSPACE="$2"
      shift 2
      ;;
    --help|-h)
      usage
      exit 0
      ;;
    *)
      error "Flag desconocido: '$1'"
      usage
      exit 1
      ;;
  esac
done

# ─── Validación de herramientas ───────────────────────────────────────────────
section "🔍 Validando dependencias"

if ! command -v terraform &>/dev/null; then
  error "terraform no está instalado o no está en el PATH."
  echo -e "  ${YELLOW}👉  Instálalo con: brew install terraform${RESET}"
  exit 1
fi
ok "terraform $(terraform version -json 2>/dev/null | grep -o '"terraform_version":"[^"]*"' | cut -d'"' -f4 || terraform version | head -1 | awk '{print $2}')"

if $FLAG_BUILD_LAMBDAS; then
  if command -v pnpm &>/dev/null; then
    PNPM_CMD="pnpm"
    ok "pnpm $(pnpm --version)"
  else
    warn "pnpm no encontrado; se usará npm para todas las lambdas."
    PNPM_CMD="npm"
  fi

  if ! command -v npm &>/dev/null; then
    error "npm no está instalado o no está en el PATH."
    echo -e "  ${YELLOW}👉  Instala Node.js desde https://nodejs.org${RESET}"
    exit 1
  fi
  ok "npm $(npm --version)"
fi

# ─── Advertencia de modo local override ──────────────────────────────────────
if [[ "$USE_LOCAL_SECRET_OVERRIDES" == "true" ]]; then
  echo -e ""
  echo -e "${RED}${BOLD}╔══════════════════════════════════════════════════════════════════╗${RESET}"
  echo -e "${RED}${BOLD}║  🔴 ADVERTENCIA: MODO LOCAL OVERRIDE ACTIVO                      ║${RESET}"
  echo -e "${RED}${BOLD}║                                                                  ║${RESET}"
  echo -e "${RED}${BOLD}║  Los secretos provienen del bloque LOCAL SECRET OVERRIDES        ║${RESET}"
  echo -e "${RED}${BOLD}║  definido directamente en terraform.sh.                          ║${RESET}"
  echo -e "${RED}${BOLD}║                                                                  ║${RESET}"
  echo -e "${RED}${BOLD}║  ⚠️  Este modo es EXCLUSIVO para desarrollo local.               ║${RESET}"
  echo -e "${RED}${BOLD}║  ⚠️  NUNCA commitees este fichero con valores reales.            ║${RESET}"
  echo -e "${RED}${BOLD}║  ⚠️  Antes de commitear: USE_LOCAL_SECRET_OVERRIDES=false        ║${RESET}"
  echo -e "${RED}${BOLD}║                         y restaurar todos los PLACEHOLDER_*      ║${RESET}"
  echo -e "${RED}${BOLD}╚══════════════════════════════════════════════════════════════════╝${RESET}"
  echo -e ""
fi

# ─── Validación de placeholders ───────────────────────────────────────────────
section "🔐 Verificando variables sensibles"

PLACEHOLDERS_FOUND=false

check_placeholder() {
  local name="$1"
  local value="$2"
  if [[ "$value" == PLACEHOLDER_* ]]; then
    warn "$name no ha sido configurado (valor: $value)"
    PLACEHOLDERS_FOUND=true
  else
    ok "$name configurado ✓"
  fi
}

check_placeholder "stripe_api_key"              "$TF_VAR_stripe_api_key"
check_placeholder "password_reset_token_secret" "$TF_VAR_password_reset_token_secret"
check_placeholder "stripe_event_bus_arn"        "$TF_VAR_stripe_event_bus_arn"

if $PLACEHOLDERS_FOUND; then
  warn "Existen placeholders sin configurar."
  if [[ "$USE_LOCAL_SECRET_OVERRIDES" == "true" ]]; then
    echo -e "  ${YELLOW}Edita el bloque LOCAL SECRET OVERRIDES en terraform.sh y sustituye los PLACEHOLDER_*.${RESET}"
  else
    echo -e "  ${YELLOW}Exporta las variables de entorno antes de ejecutar este script:${RESET}"
    echo -e "  ${YELLOW}  export TF_VAR_stripe_api_key=\"sk_live_...\"${RESET}"
    echo -e "  ${YELLOW}  export TF_VAR_stripe_webhook_secret=\"whsec_...\"${RESET}"
    echo -e "  ${YELLOW}  export TF_VAR_password_reset_token_secret=\"super-secret\"${RESET}"
    echo -e "  ${YELLOW}  export TF_VAR_stripe_event_bus_arn=\"arn:aws:events:...\"${RESET}"
    echo -e "  ${YELLOW}O activa el modo local: USE_LOCAL_SECRET_OVERRIDES=true (solo en local).${RESET}"
  fi
  echo -e "  ${YELLOW}Continuar con placeholders puede causar un deploy inválido.${RESET}"
  echo ""
  read -r -p "$(echo -e "${YELLOW}¿Continuar de todas formas? [s/N]: ${RESET}")" CONFIRM
  if [[ ! "$CONFIRM" =~ ^[sS]$ ]]; then
    info "Operación cancelada por el usuario."
    exit 0
  fi
fi

# ─── Build de lambdas (opcional) ─────────────────────────────────────────────
build_lambda() {
  local name="$1"
  local dir="$2"
  local pkg_manager="$3"

  section "🔨 Build Lambda: $name"

  if [[ ! -d "$dir" ]]; then
    error "Directorio de lambda no encontrado: $dir"
    exit 1
  fi

  info "Instalando dependencias ($pkg_manager install)..."
  (cd "$dir" && $pkg_manager install --frozen-lockfile 2>/dev/null || $pkg_manager install)
  ok "Dependencias instaladas."

  info "Compilando ($pkg_manager run build)..."
  (cd "$dir" && $pkg_manager run build)
  ok "Build completado."

  info "Empaquetando ($pkg_manager run zip)..."
  (cd "$dir" && $pkg_manager run zip)
  ok "Lambda $name empaquetada."
}

if $FLAG_BUILD_LAMBDAS; then
  section "🚀 Fase: Build de Lambdas"
  build_lambda "Payments"         "$LAMBDA_PAYMENTS_DIR"          "pnpm"
  build_lambda "SendEmail"        "$LAMBDA_SEND_EMAIL_DIR"        "pnpm"
  build_lambda "VerificationCode" "$LAMBDA_VERIFICATION_CODE_DIR" "pnpm"
  ok "Todas las lambdas compiladas y empaquetadas."
fi

# ─── Terraform init ───────────────────────────────────────────────────────────
section "🏗️  Terraform Init"
info "Inicializando Terraform en: $SCRIPT_DIR"
(cd "$SCRIPT_DIR" && terraform init)
ok "Init completado."

# ─── Terraform workspace ─────────────────────────────────────────────────────
if [[ -n "$TARGET_WORKSPACE" ]]; then
  section "🗂️  Terraform Workspace"
  info "Seleccionando workspace: ${BOLD}${TARGET_WORKSPACE}${RESET}"

  if (cd "$SCRIPT_DIR" && terraform workspace select "$TARGET_WORKSPACE" 2>/dev/null); then
    ok "Workspace '${TARGET_WORKSPACE}' seleccionado."
  else
    warn "Workspace '${TARGET_WORKSPACE}' no existe. Creándolo..."
    (cd "$SCRIPT_DIR" && terraform workspace new "$TARGET_WORKSPACE")
    ok "Workspace '${TARGET_WORKSPACE}' creado y seleccionado. 🆕"
  fi

  CURRENT_WS="$(cd "$SCRIPT_DIR" && terraform workspace show)"
  ok "Workspace activo confirmado: ${BOLD}${CURRENT_WS}${RESET} 🎯"
else
  CURRENT_WS="$(cd "$SCRIPT_DIR" && terraform workspace show 2>/dev/null || echo 'default')"
  info "Usando workspace actual: ${BOLD}${CURRENT_WS}${RESET}"
fi

# ─── Terraform apply / destroy ────────────────────────────────────────────────
if $FLAG_DESTROY; then
  section "💣 Terraform Destroy"
  warn "Se va a DESTRUIR la infraestructura del workspace '${CURRENT_WS}'."
  (cd "$SCRIPT_DIR" && terraform destroy \
    -var="stripe_api_key=${TF_VAR_stripe_api_key}" \
    -var="password_reset_token_secret=${TF_VAR_password_reset_token_secret}" \
    -var="stripe_event_bus_arn=${TF_VAR_stripe_event_bus_arn}" && terraform workspace select default && terraform workspace delete "$CURRENT_WS")
  ok "Terraform destroy completado."
else
  section "🌍 Terraform Apply"
  info "Aplicando infraestructura en workspace '${CURRENT_WS}'..."
  (cd "$SCRIPT_DIR" && terraform apply \
    -var="stripe_api_key=${TF_VAR_stripe_api_key}" \
    -var="password_reset_token_secret=${TF_VAR_password_reset_token_secret}" \
    -var="stripe_event_bus_arn=${TF_VAR_stripe_event_bus_arn}")
  ok "Terraform apply completado."
fi

section "🎉 Operación finalizada con éxito"
