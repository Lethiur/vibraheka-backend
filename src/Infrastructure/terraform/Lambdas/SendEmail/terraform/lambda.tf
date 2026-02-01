



# Pol�tica para SSM (leer par�metros)
resource "aws_iam_role_policy" "VH_ssm_read_parameters" {
  name = "ssm-read-parameters-policy"
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

# Lambda Function
resource "aws_lambda_function" "send_email" {
  function_name = "vibraheka-send-email-${terraform.workspace}"
  role          = aws_iam_role.VH_email_lambda_role.arn
  handler       = "index.handler"
  runtime       = "nodejs20.x"
  timeout       = 30
  memory_size   = 256

  filename         = "${path.module}/../lambda.zip"
  source_code_hash = filebase64sha256("${path.module}/../lambda.zip")

  environment {
    variables = {
      TEMPLATE_BUCKET          = var.template_bucket_name
      AWS_NODEJS_CONNECTION_REUSE_ENABLED = "1"
    }
  }

  tags = {
    Environment = terraform.workspace
    Application = "VibraHeka"
  }
}

# CloudWatch Log Group
resource "aws_cloudwatch_log_group" "VH_send_email_lambda_logs" {
  name              = "/aws/lambda/${aws_lambda_function.send_email.function_name}"
  retention_in_days = 7

  tags = {
    Environment = terraform.workspace
    Application = "VibraHeka"
  }
}
