module "CreateChallengeLambda" {
  source = "./Lambdas/VerificationCode/terraform"
  dynamo_codes_table_arn  = aws_dynamodb_table.VibraHeka_PAM_verification_codes.arn
  dynamo_codes_table_name = aws_dynamodb_table.VibraHeka_PAM_verification_codes.name
  kms_alias_arn           = aws_kms_alias.PAM_cognito_kms_alias.arn
  kms_alias_name          = aws_kms_alias.PAM_cognito_kms_alias.name
  kms_arn                 = aws_kms_key.VibraHeka_PAM_cognito_kms.arn
  user_pool_arn           = aws_cognito_user_pool.VibraHeka-main-pool.arn
}
