#!/usr/bin/env bash
# =============================================================================
# seed-default-admin.sh — Crea/confirma el usuario admin por defecto en
#                          Cognito e inserta su fila en DynamoDB.
#
# MODO 1 — Terraform outputs (CI):
#   ./seed-default-admin.sh --tf-outputs-json /tmp/tf-prod.json
#
# MODO 2 — Flags manuales (local / sin JSON):
#   export ADMIN_EMAIL=admin@example.com
#   export ADMIN_PASSWORD='SuperSecret123@'
#   ./seed-default-admin.sh \
#     --cognito-pool-id  eu-west-1_XXXXXXXXX \
#     --cognito-client-id XXXXXXXXXXXXXXXXXXXXXXXXXX \
#     --environment       prod
#
# MODO MIXTO — JSON + overrides manuales (los flags manuales tienen prioridad):
#   ./seed-default-admin.sh \
#     --tf-outputs-json /tmp/tf-prod.json \
#     --cognito-pool-id eu-west-1_OVERRIDE
#
# Flags disponibles:
#   --tf-outputs-json   <path>   Ruta al JSON de terraform outputs (descifrado). Opcional si se
#                                 pasan los flags manuales minimos.
#   --cognito-pool-id   <id>     Cognito User Pool ID.  Alt: env COGNITO_POOL_ID.
#   --cognito-client-id <id>     Cognito App Client ID. Alt: env COGNITO_CLIENT_ID.
#                                 (Registrado/mostrado para consistencia; no se usa en llamadas
#                                  AWS actuales del script pero puede requerirse en el futuro.)
#   --environment       <name>   Nombre de entorno (p.ej: prod, staging). Alt: env ENVIRONMENT.
#                                 Se usa para derivar el nombre de tabla si --users-table es omitido.
#   --admin-email       <email>  Email del admin. Alt: env ADMIN_EMAIL.
#   --admin-password    <pass>   Contrasena permanente. Alt: env ADMIN_PASSWORD.
#   --users-table       <table>  Nombre de la tabla DynamoDB.
#                                 Si se omite: se deriva como <environment>Users-Profiles.
#   --aws-region        <region> Region AWS. Alt: env AWS_REGION.
#   --dry-run                    Muestra lo que haria sin ejecutar nada en AWS.
#   -h, --help                   Muestra esta ayuda.
#
# Requisitos:
#   - aws CLI v2
#   - jq
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# Colores y helpers de log
# ---------------------------------------------------------------------------
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
RESET='\033[0m'

log_section() { echo -e "\n${CYAN}${BOLD}========================================${RESET}"; echo -e "${CYAN}${BOLD}  $*${RESET}"; echo -e "${CYAN}${BOLD}========================================${RESET}"; }
log_info()    { echo -e "  ${GREEN}✔${RESET}  $*"; }
log_warn()    { echo -e "  ${YELLOW}⚠${RESET}  $*"; }
log_err()     { echo -e "  ${RED}✖${RESET}  $*" >&2; }
log_dry()     { echo -e "  ${YELLOW}[DRY-RUN]${RESET}  $*"; }

die() { log_err "$*"; exit 1; }

# ---------------------------------------------------------------------------
# Valores por defecto (sobreescribibles por flags o variables de entorno)
# ---------------------------------------------------------------------------
TF_OUTPUTS_JSON_PATH=""

# Flags manuales — toman precedencia sobre los valores extraidos del JSON
MANUAL_POOL_ID="${COGNITO_POOL_ID:-}"
MANUAL_CLIENT_ID="${COGNITO_CLIENT_ID:-}"
MANUAL_ENVIRONMENT="${ENVIRONMENT:-}"

ADMIN_EMAIL="${ADMIN_EMAIL:-}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-}"
USERS_TABLE_OVERRIDE=""
AWS_REGION="${AWS_REGION:-}"
DRY_RUN=false

# ---------------------------------------------------------------------------
# Parseo de argumentos
# ---------------------------------------------------------------------------
usage() {
  sed -n '/^# MODO 1/,/^# Requisitos/p' "$0" | sed 's/^# \{0,2\}//'
  exit 0
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --tf-outputs-json)   TF_OUTPUTS_JSON_PATH="${2:-}"; shift 2 ;;
    --cognito-pool-id)   MANUAL_POOL_ID="${2:-}"; shift 2 ;;
    --cognito-client-id) MANUAL_CLIENT_ID="${2:-}"; shift 2 ;;
    --environment)       MANUAL_ENVIRONMENT="${2:-}"; shift 2 ;;
    --admin-email)       ADMIN_EMAIL="${2:-}"; shift 2 ;;
    --admin-password)    ADMIN_PASSWORD="${2:-}"; shift 2 ;;
    --users-table)       USERS_TABLE_OVERRIDE="${2:-}"; shift 2 ;;
    --aws-region)        AWS_REGION="${2:-}"; shift 2 ;;
    --dry-run)           DRY_RUN=true; shift ;;
    -h|--help)           usage ;;
    *) die "Argumento desconocido: '$1'. Usa --help para ver la ayuda." ;;
  esac
done

# ---------------------------------------------------------------------------
# Comprobacion de dependencias
# ---------------------------------------------------------------------------
log_section "Comprobando dependencias"

check_dep() {
  if ! command -v "$1" &>/dev/null; then
    log_err "Dependencia no encontrada: '$1'."
    case "$1" in
      aws)  log_err "Instala AWS CLI v2: https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html" ;;
      jq)   log_err "Instala jq: brew install jq  (macOS)  |  sudo apt-get install -y jq  (Ubuntu)" ;;
    esac
    exit 1
  fi
}

check_dep aws
check_dep jq
log_info "aws CLI: $(aws --version 2>&1 | head -n1)"
log_info "jq:      $(jq --version)"

# ---------------------------------------------------------------------------
# Validacion de argumentos de credenciales de admin (comun a ambos modos)
# ---------------------------------------------------------------------------
log_section "Validando parametros"

[[ -z "$ADMIN_EMAIL" ]]    && die "Email de admin no especificado. Usa --admin-email o exporta ADMIN_EMAIL."
[[ -z "$ADMIN_PASSWORD" ]] && die "Contrasena de admin no especificada. Usa --admin-password o exporta ADMIN_PASSWORD."

# ---------------------------------------------------------------------------
# Resolucion de valores de infraestructura
#
# Precedencia (mayor a menor):
#   1. Flags manuales (--cognito-pool-id, --cognito-client-id, --environment)
#      o variables de entorno equivalentes (COGNITO_POOL_ID, etc.)
#   2. Valores extraidos del JSON de terraform outputs
#
# Razon: en entornos locales el JSON puede no estar disponible; los flags
# manuales permiten operar sin el. En modo mixto, los flags permiten
# sobreescribir valores puntuales del JSON sin regenerarlo.
# ---------------------------------------------------------------------------

# Valores que se resolveran finalmente
USER_POOL_ID=""
CLIENT_ID=""
RESOLVED_ENVIRONMENT=""

# --- Paso A: leer JSON si se proporciono ---
JSON_POOL_ID=""
JSON_CLIENT_ID=""
JSON_ENVIRONMENT=""
JSON_USERS_TABLE=""

if [[ -n "$TF_OUTPUTS_JSON_PATH" ]]; then
  [[ ! -f "$TF_OUTPUTS_JSON_PATH" ]] && die "Fichero de terraform outputs no encontrado: '$TF_OUTPUTS_JSON_PATH'"

  if ! jq empty "$TF_OUTPUTS_JSON_PATH" 2>/dev/null; then
    die "El fichero '$TF_OUTPUTS_JSON_PATH' no es JSON valido."
  fi

  log_info "Leyendo outputs de Terraform desde: $TF_OUTPUTS_JSON_PATH"
  JSON_POOL_ID="$(jq -r '.cognito_pool_id.value // empty' "$TF_OUTPUTS_JSON_PATH")"
  JSON_CLIENT_ID="$(jq -r '.cognito_client_id.value // empty' "$TF_OUTPUTS_JSON_PATH")"
  JSON_ENVIRONMENT="$(jq -r '.environment.value // empty' "$TF_OUTPUTS_JSON_PATH")"
  JSON_USERS_TABLE="$(jq -r '.users_table_name.value // empty' "$TF_OUTPUTS_JSON_PATH")"
fi

# --- Paso B: aplicar precedencia (manual > JSON) ---
if [[ -n "$MANUAL_POOL_ID" ]]; then
  USER_POOL_ID="$MANUAL_POOL_ID"
  [[ -n "$JSON_POOL_ID" && "$JSON_POOL_ID" != "$MANUAL_POOL_ID" ]] && \
    log_warn "cognito-pool-id: se usa valor manual ('$MANUAL_POOL_ID') en lugar del JSON ('$JSON_POOL_ID')."
else
  USER_POOL_ID="$JSON_POOL_ID"
fi

if [[ -n "$MANUAL_CLIENT_ID" ]]; then
  CLIENT_ID="$MANUAL_CLIENT_ID"
  [[ -n "$JSON_CLIENT_ID" && "$JSON_CLIENT_ID" != "$MANUAL_CLIENT_ID" ]] && \
    log_warn "cognito-client-id: se usa valor manual en lugar del JSON."
else
  CLIENT_ID="$JSON_CLIENT_ID"
fi

if [[ -n "$MANUAL_ENVIRONMENT" ]]; then
  RESOLVED_ENVIRONMENT="$MANUAL_ENVIRONMENT"
  [[ -n "$JSON_ENVIRONMENT" && "$JSON_ENVIRONMENT" != "$MANUAL_ENVIRONMENT" ]] && \
    log_warn "environment: se usa valor manual ('$MANUAL_ENVIRONMENT') en lugar del JSON ('$JSON_ENVIRONMENT')."
else
  RESOLVED_ENVIRONMENT="$JSON_ENVIRONMENT"
fi

# --- Paso C: validar que tenemos lo minimo necesario ---
[[ -z "$USER_POOL_ID" ]] && die "Cognito User Pool ID no resuelto. Usa --cognito-pool-id <id>, exporta COGNITO_POOL_ID, o proporciona --tf-outputs-json con 'cognito_pool_id'."

# --- Paso D: resolver nombre de tabla DynamoDB ---
if [[ -n "$USERS_TABLE_OVERRIDE" ]]; then
  USERS_TABLE="$USERS_TABLE_OVERRIDE"
elif [[ -n "$JSON_USERS_TABLE" && -z "$MANUAL_ENVIRONMENT" ]]; then
  # Solo usamos el nombre de tabla del JSON si no se ha especificado environment manual
  # (el environment manual implica intención de derivar el nombre localmente)
  USERS_TABLE="$JSON_USERS_TABLE"
elif [[ -n "$RESOLVED_ENVIRONMENT" ]]; then
  USERS_TABLE="${RESOLVED_ENVIRONMENT}Users-Profiles"
else
  die "No se pudo resolver el nombre de la tabla DynamoDB. Usa --users-table <tabla>, --environment <env> o incluye 'environment.value' en los outputs JSON."
fi

# ---------------------------------------------------------------------------
# Resumen de configuracion resuelta
# ---------------------------------------------------------------------------
log_info "Cognito User Pool ID : $USER_POOL_ID"
# CLIENT_ID se registra para consistencia de configuracion aunque no se use
# en las llamadas AWS actuales (admin-create-user / admin-set-user-password).
# Se conserva para facilitar su uso futuro (p.ej. InitiateAuth, token exchange).
[[ -n "$CLIENT_ID" ]] && log_info "Cognito Client ID    : $CLIENT_ID" || log_warn "Cognito Client ID    : no proporcionado (no requerido para acciones actuales)."
log_info "Environment          : ${RESOLVED_ENVIRONMENT:-(no especificado)}"
log_info "DynamoDB Users Table : $USERS_TABLE"
log_info "Admin email          : $ADMIN_EMAIL"
log_info "AWS Region           : ${AWS_REGION:-(variable de entorno / perfil AWS)}"
[[ "$DRY_RUN" == true ]] && log_warn "Modo DRY-RUN activo — ninguna llamada AWS sera ejecutada."

# ---------------------------------------------------------------------------
# Helpers para ejecucion en dry-run
# ---------------------------------------------------------------------------
aws_exec() {
  if [[ "$DRY_RUN" == true ]]; then
    log_dry "aws $*"
    return 0
  fi
  aws "$@"
}

# ---------------------------------------------------------------------------
# Construir args de region (solo si se especifico)
# ---------------------------------------------------------------------------
REGION_ARGS=()
[[ -n "$AWS_REGION" ]] && REGION_ARGS=(--region "$AWS_REGION")

# ---------------------------------------------------------------------------
# Paso 1 — Verificar conectividad AWS
# ---------------------------------------------------------------------------
log_section "Verificando credenciales AWS"

if [[ "$DRY_RUN" != true ]]; then
  if ! CALLER_ID="$(aws sts get-caller-identity --output json ${REGION_ARGS[@]+"${REGION_ARGS[@]}"} 2>&1)"; then
    die "No se pudo verificar la identidad AWS. Comprueba tus credenciales.\nDetalle: $CALLER_ID"
  fi
  AWS_ACCOUNT="$(echo "$CALLER_ID" | jq -r '.Account')"
  AWS_ARN="$(echo "$CALLER_ID" | jq -r '.Arn')"
  log_info "Account : $AWS_ACCOUNT"
  log_info "ARN     : $AWS_ARN"
else
  log_dry "aws sts get-caller-identity"
  log_info "Credenciales no verificadas en modo dry-run."
fi

# ---------------------------------------------------------------------------
# Paso 2 — Crear usuario Cognito (idempotente)
# ---------------------------------------------------------------------------
log_section "Creando usuario Cognito (idempotente)"

USER_EXISTS=false
if [[ "$DRY_RUN" != true ]]; then
  if aws cognito-idp admin-get-user \
       --user-pool-id "$USER_POOL_ID" \
       --username "$ADMIN_EMAIL" \
       ${REGION_ARGS[@]+"${REGION_ARGS[@]}"} >/dev/null 2>&1; then
    USER_EXISTS=true
    log_warn "El usuario '$ADMIN_EMAIL' ya existe en Cognito — se omite la creacion."
  fi
fi

if [[ "$USER_EXISTS" == false ]]; then
  log_info "Creando usuario '$ADMIN_EMAIL' en Cognito..."
  aws_exec cognito-idp admin-create-user \
    --user-pool-id "$USER_POOL_ID" \
    --username "$ADMIN_EMAIL" \
    --user-attributes \
      Name=email,Value="$ADMIN_EMAIL" \
      Name=email_verified,Value=true \
      Name=name,Value="Default Admin" \
    --message-action SUPPRESS \
    ${REGION_ARGS[@]+"${REGION_ARGS[@]}"}
  log_info "Usuario creado."
fi

# ---------------------------------------------------------------------------
# Paso 3 — Establecer contrasena permanente
# ---------------------------------------------------------------------------
log_section "Estableciendo contrasena permanente"

aws_exec cognito-idp admin-set-user-password \
  --user-pool-id "$USER_POOL_ID" \
  --username "$ADMIN_EMAIL" \
  --password "$ADMIN_PASSWORD" \
  --permanent \
  ${REGION_ARGS[@]+"${REGION_ARGS[@]}"}
log_info "Contrasena establecida como permanente."

# ---------------------------------------------------------------------------
# Paso 4 — Obtener SUB del usuario
# ---------------------------------------------------------------------------
log_section "Obteniendo SUB del usuario"

if [[ "$DRY_RUN" == true ]]; then
  SUB="dry-run-sub-placeholder"
  log_dry "SUB = $SUB"
else
  SUB="$(aws cognito-idp admin-get-user \
    --user-pool-id "$USER_POOL_ID" \
    --username "$ADMIN_EMAIL" \
    --query "UserAttributes[?Name=='sub']|[0].Value" \
    --output text \
    ${REGION_ARGS[@]+"${REGION_ARGS[@]}"})"
  if [[ -z "$SUB" || "$SUB" == "None" ]]; then
    die "No se pudo obtener el SUB del usuario '$ADMIN_EMAIL'."
  fi
  log_info "SUB : $SUB"
fi

# ---------------------------------------------------------------------------
# Paso 5 — Insertar/actualizar fila admin en DynamoDB (idempotente)
# ---------------------------------------------------------------------------
log_section "Seeding fila admin en DynamoDB"

NOW="$(date -u +"%Y-%m-%dT%H:%M:%S.0000000+00:00")"

DYNAMO_ITEM=$(jq -nc \
  --arg id      "$SUB" \
  --arg email   "$ADMIN_EMAIL" \
  --arg now     "$NOW" \
  '{
    "Id":                 { "S": $id },
    "CustomerID":         { "S": "" },
    "Email":              { "S": $email },
    "FirstName":          { "S": "Default" },
    "MiddleName":         { "S": "" },
    "LastName":           { "S": "Admin" },
    "PhoneNumber":        { "S": "+34600000000" },
    "Bio":                { "S": "System default administrator" },
    "TimezoneID":         { "S": "Europe/Madrid" },
    "ProfilePictureUrl":  { "S": "" },
    "Role":               { "S": "Admin" },
    "Created":            { "S": $now },
    "CreatedBy":          { "S": $id },
    "LastModified":       { "S": $now },
    "LastModifiedBy":     { "S": $id }
  }')

log_info "Insertando en tabla '$USERS_TABLE'..."
aws_exec dynamodb put-item \
  --table-name "$USERS_TABLE" \
  --item "$DYNAMO_ITEM" \
  ${REGION_ARGS[@]+"${REGION_ARGS[@]}"}
log_info "Fila admin insertada/actualizada correctamente."

# ---------------------------------------------------------------------------
# Resumen final
# ---------------------------------------------------------------------------
log_section "Seed completado"
log_info "Usuario admin '$ADMIN_EMAIL' listo en Cognito y DynamoDB."
[[ "$DRY_RUN" == true ]] && log_warn "Recuerda: modo dry-run activo — no se realizaron cambios reales."
echo ""

