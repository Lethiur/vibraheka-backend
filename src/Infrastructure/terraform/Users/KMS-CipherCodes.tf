resource "aws_kms_key" "VibraHeka_PAM_cognito_kms" {
  description             = "KMS key for Cognito custom email sender for env ${terraform.workspace}"
  deletion_window_in_days = 7
  enable_key_rotation     = false
  tags = merge(local.tags, {
    "Name" : "${local.table_prefix}KMSKey"
  })
}

resource "aws_kms_alias" "PAM_cognito_kms_alias" {
  name = "alias/${local.table_prefix}cognito-email-sender"
  target_key_id = aws_kms_key.VibraHeka_PAM_cognito_kms.key_id
}

output "kms_users_arn" {
  value = aws_kms_key.VibraHeka_PAM_cognito_kms.arn
}

output "kms_users_key_alias_arn" {
  value = aws_kms_alias.PAM_cognito_kms_alias.arn
}

output "kms_users_key_alias_name" {
  value = aws_kms_alias.PAM_cognito_kms_alias.name
}