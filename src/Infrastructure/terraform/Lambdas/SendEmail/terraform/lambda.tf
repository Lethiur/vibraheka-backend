resource "aws_iam_role_policy" "VH_ssm_read_parameters" {
  name = "ssm-read-parameters-policy-${terraform.workspace}"
  role = aws_iam_role.VH_email_lambda_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "ssm:GetParametersByPath",
          "ssm:GetParameter",
          "ssm:GetParameters"
        ]
        Effect   = "Allow"
        Resource = "arn:aws:ssm:*:*:parameter/VibraHeka/*"
      }
    ]
  })
}

locals {
  send_email_environment_variables = {
    TEMPLATE_BUCKET                           = var.template_bucket_name
    SES_FROM_EMAIL                            = var.ses_email_from
    SES_CONFIG_SET                            = var.ses_config_set_name
    SSM_TEMPLATE_NAME_PARAM                   = var.ssm_verification_template_param
    SSM_VERIFICATION_TEMPLATE_NAME_PARAM      = var.ssm_verification_template_param
    SSM_PASSWORD_RESET_TEMPLATE_NAME_PARAM    = var.ssm_password_reset_template_param
    SSM_SUBSCRIPTION_THANK_YOU_TEMPLATE_NAME_PARAM = var.ssm_subscription_thank_you_template_param
    SSM_TRIAL_ENDING_SOON_TEMPLATE_NAME_PARAM = var.ssm_trial_ending_soon_template_param
    PASSWORD_RESET_TOKEN_SECRET               = var.password_reset_token_secret
    PASSWORD_RESET_FRONTEND_URL               = var.password_reset_frontend_url
    PASSWORD_RESET_TOKEN_TTL_MINUTES          = tostring(var.password_reset_token_ttl_minutes)
    KEY_ARN                                   = var.kms_arn
    KEY_ALIAS                                 = var.kms_alias_name
    AWS_NODEJS_CONNECTION_REUSE_ENABLED       = "1"
  }
}

resource "aws_lambda_function" "send_email_cognito" {
  function_name = "vibraheka-send-email-cognito-${terraform.workspace}"
  role          = aws_iam_role.VH_email_lambda_role.arn
  handler       = "lambda_send_email_cognito.handler"
  runtime       = "nodejs24.x"
  timeout       = 30
  memory_size   = 256

  filename         = "${path.module}/../lambda-cognito.zip"
  source_code_hash = filebase64sha256("${path.module}/../lambda-cognito.zip")

  environment {
    variables = local.send_email_environment_variables
  }

  tags = {
    Environment = terraform.workspace
    Application = "VibraHeka"
  }
}

resource "aws_lambda_function" "send_email_notifications" {
  function_name = "vibraheka-send-email-notifications-${terraform.workspace}"
  role          = aws_iam_role.VH_email_lambda_role.arn
  handler       = "lambda_send_email_notifications.handler"
  runtime       = "nodejs24.x"
  timeout       = 30
  memory_size   = 256

  filename         = "${path.module}/../lambda-notifications.zip"
  source_code_hash = filebase64sha256("${path.module}/../lambda-notifications.zip")

  environment {
    variables = local.send_email_environment_variables
  }

  tags = {
    Environment = terraform.workspace
    Application = "VibraHeka"
  }
}

resource "aws_cloudwatch_log_group" "VH_send_email_cognito_lambda_logs" {
  name              = "/aws/lambda/${aws_lambda_function.send_email_cognito.function_name}"
  retention_in_days = 7

  tags = {
    Environment = terraform.workspace
    Application = "VibraHeka"
  }
}

resource "aws_cloudwatch_log_group" "VH_send_email_notifications_lambda_logs" {
  name              = "/aws/lambda/${aws_lambda_function.send_email_notifications.function_name}"
  retention_in_days = 7

  tags = {
    Environment = terraform.workspace
    Application = "VibraHeka"
  }
}

output "lambda_send_email_arn" {
  value = aws_lambda_function.send_email_cognito.arn
}

output "notification_event_bus_name" {
  value = aws_cloudwatch_event_bus.notifications.name
}

output "notification_event_bus_arn" {
  value = aws_cloudwatch_event_bus.notifications.arn
}
