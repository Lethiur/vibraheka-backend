###############################
# Dynamo DB policy
###############################

resource "aws_iam_policy" "Lambda_PAM_Verification_codes_dynamodb_policy" {
  name = "VibraHeka-LambdaDynamoDBPolicy-${terraform.workspace}"
  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Action = ["dynamodb:PutItem"],
        Effect   = "Allow",
        Resource = var.dynamo_codes_table_arn
      }
    ]
  })
  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system: "PAM",
    service : "email-sending",
    dev : terraform.workspace != "prod"
  }
}

resource "aws_iam_policy_attachment" "Lambda_PAM_Verification_codes_dynamodb_policy_attach" {
  name       = "lambda_dynamodb_policy_attachment-${terraform.workspace}"
  roles = [aws_iam_role.PAM_IAM_lambda_exec.name]
  policy_arn = aws_iam_policy.Lambda_PAM_Verification_codes_dynamodb_policy.arn
}
