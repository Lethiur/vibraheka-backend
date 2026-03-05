#!/usr/bin/env bash
set -euo pipefail

TERRAFORM_DIR="${1:-src/Infrastructure/terraform}"
ARG2="${2:-}"
ARG3="${3:-}"

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

if [[ -n "$WORKSPACE" ]]; then
  terraform -chdir="$TERRAFORM_DIR" workspace select "$WORKSPACE" >/dev/null
fi

TF_OUTPUTS_JSON="$(terraform -chdir="$TERRAFORM_DIR" output -json)"
PASSWORD_RESET_TOKEN_SECRET_VALUE="${PASSWORD_RESET_TOKEN_SECRET:-}"

for appsettings_path in "${APPSETTINGS_PATHS[@]}"; do
  tmp_file="$(mktemp)"
  jq --argjson tf "$TF_OUTPUTS_JSON" --arg prs "$PASSWORD_RESET_TOKEN_SECRET_VALUE" '
    .AWS = (.AWS // {}) |
    .Backend = (.Backend // {}) |

    (if ($tf.users_table_name.value? != null) then .AWS.UsersTable = $tf.users_table_name.value else . end) |
    (if ($tf.verification_codes_table_name.value? != null) then .AWS.CodesTable = $tf.verification_codes_table_name.value else . end) |
    (if ($tf.email_templates_table_name.value? != null) then .AWS.EmailTemplatesTable = $tf.email_templates_table_name.value else . end) |
    (if ($tf.email_templates_bucket_name.value? != null) then .AWS.EmailTemplatesBucketName = $tf.email_templates_bucket_name.value else . end) |
    (if ($tf.action_log_table_name.value? != null) then .AWS.ActionLogTable = $tf.action_log_table_name.value else . end) |
    (if ($tf.subscriptions_table_name.value? != null) then .AWS.SubscriptionTable = $tf.subscriptions_table_name.value else . end) |
    (if ($tf.subscriptions_table_user_index_name.value? != null) then .AWS.SubscriptionUserIdIndex = $tf.subscriptions_table_user_index_name.value else . end) |
    (if ($tf.cognito_client_id.value? != null) then .AWS.ClientId = $tf.cognito_client_id.value else . end) |
    (if ($tf.cognito_pool_id.value? != null) then .AWS.UserPoolId = $tf.cognito_pool_id.value else . end) |
    (if ($tf.backend_api_gateway_base_route.value? != null) then .AWS.ApiGatewayBaseUrl = $tf.backend_api_gateway_base_route.value else . end) |
    (if ($tf.settings_namespace.value? != null) then .AWS.SettingsNameSpace = $tf.settings_namespace.value else . end) |
    (if ($prs != "") then .AWS.PasswordResetTokenSecret = $prs else . end) |

    (if ($tf.backend_api_gateway_base_route.value? != null) then .Backend.ApiGatewayBaseUrl = $tf.backend_api_gateway_base_route.value else . end) |
    (if ($tf.backend_api_gateway_endpoint.value? != null) then .Backend.ApiGatewayEndpoint = $tf.backend_api_gateway_endpoint.value else . end) |
    (if ($tf.backend_ecr_repository_url.value? != null) then .Backend.EcrRepositoryUrl = $tf.backend_ecr_repository_url.value else . end)
  ' "$appsettings_path" > "$tmp_file"

  mv "$tmp_file" "$appsettings_path"
  echo "Updated appsettings file: $appsettings_path"
done
