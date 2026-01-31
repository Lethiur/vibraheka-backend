
resource "aws_lambda_function" "custom_message" {
  function_name = "cognito-custom-message"
  role          = aws_iam_role.lambda_role.arn
  handler       = "index.handler"
  runtime       = "nodejs18.x"

  filename         = "lambda.zip"
  source_code_hash = filebase64sha256("lambda.zip")

  environment {
    variables = {
      TEMPLATE_BUCKET = "mi-bucket-templates"
      SES_FROM_EMAIL  = "no-reply@midominio.com"
    }
  }
}