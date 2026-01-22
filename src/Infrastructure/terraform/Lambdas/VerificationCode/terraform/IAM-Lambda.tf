###############################
# IAM Resources for Lambda
###############################

resource "aws_iam_role" "PAM_IAM_lambda_exec" {
  name  = substr("cognito_custom_message_role-${terraform.workspace}",0,63)
  assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Action = "sts:AssumeRole",
        Effect = "Allow",
        Principal = { Service = "lambda.amazonaws.com" }
      }
    ]
  })
  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system: "VibraHeka",
    service : "PAM",
    dev : terraform.workspace != "prod"
  }
}