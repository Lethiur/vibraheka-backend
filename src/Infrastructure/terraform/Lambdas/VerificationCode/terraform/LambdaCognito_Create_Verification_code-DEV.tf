###############################
# Custom Message Lambda Function
###############################

resource "aws_lambda_function" "create_challenge" {
  function_name = substr("VH_cognito_create_challenge_${terraform.workspace}", 0,63)
  runtime = "nodejs20.x"   # Cost‑efficient runtime
  role = aws_iam_role.PAM_IAM_lambda_exec.arn
  kms_key_arn   = var.kms_arn
  handler       = "lambda_create.lambda_handler"
  filename = "${path.module}/../lambda_create.zip"  # Zip package containing lambda_function.py
  source_code_hash = filebase64sha256("${path.module}/../lambda_create.zip")
  timeout = 600

  environment {
    variables = {
      DYNAMO_TABLE_NAME = var.dynamo_codes_table_name
      KEY_ARN           = var.kms_arn
      KEY_ALIAS         = var.kms_alias_name
    }
  }
  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system: "VibraHeka",
    service : "PAM",
    dev : terraform.workspace != "prod"
  }
}

###############################
# Lambda Permission for Cognito
###############################

resource "aws_lambda_permission" "allow_cognito_create_challenge" {
  statement_id  = "AllowExecutionFromCognito"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.create_challenge.function_name
  principal     = "cognito-idp.amazonaws.com"
  source_arn    = var.user_pool_arn
}


output "lambda_arn" {
  value = aws_lambda_function.create_challenge.arn
}