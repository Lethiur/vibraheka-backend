###############################
# KMS Policy
###############################
resource "aws_iam_policy" "kms_policy" {
  name  = "KMSPolicy-${terraform.workspace}"
  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Action = ["kms:Decrypt"],
        Resource = [var.kms_arn,var.kms_alias_arn],
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


resource "aws_iam_policy_attachment" "PAM_lambda_kms_policy_attach" {
  name       = "lambda_kms_policy_attachment-${terraform.workspace}"
  roles = [aws_iam_role.PAM_IAM_lambda_exec.name]
  policy_arn = aws_iam_policy.kms_policy.arn
}
