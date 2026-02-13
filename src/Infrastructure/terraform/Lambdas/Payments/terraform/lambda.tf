resource "aws_iam_role" "lambda_role" {
  name = "stripe_lambda_role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action    = "sts:AssumeRole"
        Effect    = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_basic" {
  role       = aws_iam_role.lambda_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_lambda_function" "stripe_lambda" {
  function_name = "stripe_event_lambda"
  role          = aws_iam_role.lambda_role.arn
  runtime       = "nodejs24.x"
  handler       = "lambda_stripe.handler"

  filename =  "${path.module}/../lambda.zip"
  source_code_hash = filebase64sha256("${path.module}/../lambda.zip")

  environment {
    variables = {
      STRIPE_SECRET_KEY     = var.stripe_secret_key
    }
  }

  tags = {
    Environment = terraform.workspace
    Application = "VibraHeka"
  }
}

