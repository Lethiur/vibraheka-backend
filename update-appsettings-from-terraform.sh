#!/usr/bin/env bash
set -euo pipefail

TF_OUTPUTS_JSON_PATH=""

if [[ "${1:-}" == "--tf-outputs-json" ]]; then
  TF_OUTPUTS_JSON_PATH="${2:-}"
  shift 2
fi

TERRAFORM_DIR="src/Infrastructure/terraform"

# Backwards compatible argument parsing:
# - Historically arg1 was TERRAFORM_DIR. When using --tf-outputs-json, TERRAFORM_DIR is optional.
if [[ -n "${1:-}" && ( -d "${1}" || "${1}" == */* || "${1}" == ./* || "${1}" == ../* ) ]]; then
  TERRAFORM_DIR="$1"
  shift 1
fi

ARG2="${1:-}"
ARG3="${2:-}"

DEFAULT_APPSETTINGS_PATHS=(
  "src/Web/appsettings.json"
  "src/Web/appsettings.Development.json"
  "src/Web/appsettings.Test.json"
)

APPSETTINGS_PATHS=("${DEFAULT_APPSETTINGS_PATHS[@]}")
WORKSPACE=""

# Backwards compatibility:
# - If 2nd arg is *.json => single appsettings path, optional workspace in 3rd arg.
# - Otherwise 2nd arg is workspace and default appsettings paths are used.
if [[ -n "$ARG2" && "$ARG2" == *.json ]]; then
  APPSETTINGS_PATHS=("$ARG2")
  WORKSPACE="$ARG3"
else
  WORKSPACE="$ARG2"
fi

for path in "${APPSETTINGS_PATHS[@]}"; do
  if [[ ! -f "$path" ]]; then
    echo "AppSettings file not found at path: $path" >&2
    exit 1
  fi
done

if [[ -n "$TF_OUTPUTS_JSON_PATH" ]]; then
  if [[ ! -f "$TF_OUTPUTS_JSON_PATH" ]]; then
    echo "Terraform outputs JSON file not found at path: $TF_OUTPUTS_JSON_PATH" >&2
    exit 1
  fi
  TF_OUTPUTS_JSON="$(cat "$TF_OUTPUTS_JSON_PATH")"
else
  if [[ -n "$WORKSPACE" ]]; then
    terraform -chdir="$TERRAFORM_DIR" workspace select "$WORKSPACE" >/dev/null
  fi
  TF_OUTPUTS_JSON="$(terraform -chdir="$TERRAFORM_DIR" output -json)"
fi
PASSWORD_RESET_TOKEN_SECRET_VALUE="${PASSWORD_RESET_TOKEN_SECRET:-}"
STRIPE_SECRET_KEY_VALUE="${STRIPE_SECRET_KEY:-}"
AWS_REGION="${REGION:-}"
AWS_PROFILE="${PROFILE:-}"
ZOOM_ACCOUNT_ID="${ZOOM_ACCOUNT_ID:-}"
ZOOM_CLIENT_ID="${ZOOM_CLIENT_ID:-}"
ZOOM_CLIENT_SECRET="${ZOOM_CLIENT_SECRET:-}"
ZOOM_EMAIL="${ZOOM_EMAIL:-}"


for appsettings_path in "${APPSETTINGS_PATHS[@]}"; do
  tmp_file="$(mktemp)"
  jq \
    --argjson tf "$TF_OUTPUTS_JSON" \
    --arg prof "$AWS_PROFILE" \
    --arg prs "$PASSWORD_RESET_TOKEN_SECRET_VALUE" \
    --arg ssk "$STRIPE_SECRET_KEY_VALUE" \
    --arg reg "$AWS_REGION" \
    --arg zoomAccountId "$ZOOM_ACCOUNT_ID" \
    --arg zoomClientId "$ZOOM_CLIENT_ID" \
    --arg zoomClientSecret "$ZOOM_CLIENT_SECRET" \
    --arg zoomEmail "$ZOOM_EMAIL" \
    '
    .AWS = (.AWS // {}) |
    .Stripe = (.Stripe // {}) |
    (if ($tf.email_templates_bucket_name.value? != null) then .AWS.EmailTemplatesBucketName = $tf.email_templates_bucket_name.value else . end) |
    (if ($tf.recordings_bucket_name.value? != null) then .AWS.RecordingsBucketName = $tf.recordings_bucket_name.value else . end) |
    (if ($tf.cognito_client_id.value? != null) then .AWS.ClientId = $tf.cognito_client_id.value else . end) |
    (if ($tf.cognito_pool_id.value? != null) then .AWS.UserPoolId = $tf.cognito_pool_id.value else . end) |
    (if ($tf.settings_namespace.value? != null) then .AWS.SettingsNameSpace = $tf.settings_namespace.value else . end) |
    (if ($prs != "") then .AWS.PasswordResetTokenSecret = $prs else . end) |
    (if ($ssk != "") then .Stripe.SecretKey = $ssk else . end) |
    .AWS.Environment = $tf.environment.value |
    .AWS.Profile = $prof |
    .AWS.Location = $reg |
    .AWSLogging.LogGroup = ($tf.settings_namespace.value + "api/logs") |
    .AWSLogging.Region = $reg |
    .AWSLogging.Profile = $prof |
    .XRay.ServiceName = $tf.project_name.value
  ' "$appsettings_path" > "$tmp_file"

  mv "$tmp_file" "$appsettings_path"
  echo "Updated appsettings file: $appsettings_path"
done
