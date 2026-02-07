resource "aws_kms_key" "VibraHeka_PAM_cognito_kms" {
  description             = "KMS key for Cognito custom email sender"
  deletion_window_in_days = 7
  enable_key_rotation     = false
  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system: "VibraHeka",
    service : "PAM",
    dev : terraform.workspace != "prod"
  }
}

resource "aws_kms_alias" "PAM_cognito_kms_alias" {
  name = "alias/vibra-heka-cognito-email-sender-${terraform.workspace}"
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