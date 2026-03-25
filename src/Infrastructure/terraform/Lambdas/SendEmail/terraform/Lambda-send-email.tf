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

resource "aws_cloudwatch_log_group" "VH_send_email_cognito_lambda_logs" {
  name              = "/aws/lambda/${aws_lambda_function.send_email_cognito.function_name}"
  retention_in_days = 7

  tags = {
    Environment = terraform.workspace
    Application = "VibraHeka"
  }
}
