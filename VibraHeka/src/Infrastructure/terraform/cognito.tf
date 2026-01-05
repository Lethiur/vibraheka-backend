resource "aws_cognito_user_pool" "VibraHeka-main-pool" {
  name = "VH-user-pool-${terraform.workspace}"

  auto_verified_attributes = ["email"]

  schema {
    name = "email"
    attribute_data_type = "String"
    required = true
  }
  
  
  email_configuration {
    email_sending_account = "DEVELOPER"
    from_email_address    = "VibraHeka <no-reply@vibraheka.com>"
    source_arn            = aws_ses_domain_identity.VibraHeka_ses_domain.arn
    reply_to_email_address = "support@vibraheka.com"
  }
  
  lambda_config {
    kms_key_id = aws_kms_key.VibraHeka_PAM_cognito_kms.arn
    custom_email_sender {
      lambda_arn     = module.CreateChallengeLambda.lambda_arn
      lambda_version = "V1_0"
    }
  }
  
  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system: "VibraHeka",
    service : "PAM",
    dev : terraform.workspace != "prod"
  }

  schema {
    name = "name"
    attribute_data_type = "String"
    required = true
  }
}

resource "aws_cognito_user_pool_client" "PAM_cognito_pool_client" {
  name         = "VH-client"
  user_pool_id = aws_cognito_user_pool.VibraHeka-main-pool.id
  generate_secret = false
  explicit_auth_flows = [
    "ALLOW_ADMIN_USER_PASSWORD_AUTH", 
    "ALLOW_CUSTOM_AUTH",
    "ALLOW_USER_PASSWORD_AUTH",      
    "ALLOW_REFRESH_TOKEN_AUTH"       
  ]
}

output "user_pool_id" {
  value = aws_cognito_user_pool.VibraHeka-main-pool.id
}

output "client_id" {
  value = aws_cognito_user_pool_client.PAM_cognito_pool_client.id
}

