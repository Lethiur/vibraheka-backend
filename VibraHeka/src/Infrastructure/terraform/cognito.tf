resource "aws_cognito_user_pool" "main" {
  name = "therapy-user-pool"

  auto_verified_attributes = ["email"]

  schema {
    name = "email"
    attribute_data_type = "String"
    required = true
  }

  schema {
    name = "name"
    attribute_data_type = "String"
    required = true
  }
}

resource "aws_cognito_user_pool_client" "app_client" {
  name         = "therapy-client"
  user_pool_id = aws_cognito_user_pool.main.id
  generate_secret = false
  explicit_auth_flows = [
    "ALLOW_USER_PASSWORD_AUTH",
    "ALLOW_REFRESH_TOKEN_AUTH"
  ]
}

output "user_pool_id" {
  value = aws_cognito_user_pool.main.id
}

output "client_id" {
  value = aws_cognito_user_pool_client.app_client.id
}

