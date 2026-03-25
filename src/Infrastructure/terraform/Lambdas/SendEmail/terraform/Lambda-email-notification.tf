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

resource "aws_cloudwatch_log_group" "VH_send_email_notifications_lambda_logs" {
  name              = "/aws/lambda/${aws_lambda_function.send_email_notifications.function_name}"
  retention_in_days = 7

  tags = {
    Environment = terraform.workspace
    Application = "VibraHeka"
  }
}