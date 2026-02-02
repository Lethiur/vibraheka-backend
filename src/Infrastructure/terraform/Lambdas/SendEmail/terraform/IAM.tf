resource "aws_iam_role" "VH_email_lambda_role" {
  name = "vibraheka-send-email-lambda-role-${terraform.workspace}"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })
  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system : "VibraHeka",
    service : "PAM",
    dev : terraform.workspace != "prod"
  }
}

resource "aws_iam_role_policy_attachment" "VH_email_lambda_logs" {
  role       = aws_iam_role.VH_email_lambda_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_iam_role_policy" "VH_ses_send_email_policy" {
  name = "ses-send-email-policy-${terraform.workspace}"
  role = aws_iam_role.VH_email_lambda_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "ses:SendEmail",
          "ses:SendTemplatedEmail",
          "ses:SendRawEmail"
        ]
        Resource = [var.ses-arn, var.ses-domain-arn, var.ses_config_set_arn]
      }
    ]
  })
}


resource "aws_iam_role_policy" "VH_s3_bucket_access" {
  name = "send-email-s3-read-templates-policy-${terraform.workspace}"
  role = aws_iam_role.VH_email_lambda_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "s3:GetObject",
          "s3:ListBucket"
        ]
        Resource = [
          var.template_bucket_arn,
          "${var.template_bucket_arn}/*"
        ]
      }
    ]
  })
}

resource "aws_iam_policy" "kms_policy" {
  name = "SendEmail-KMSPolicy-${terraform.workspace}"
  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect   = "Allow",
        Action   = ["kms:Decrypt"],
        Resource = [var.kms_arn, var.kms_alias_arn],
      }
    ]
  })
  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system : "VibraHeka",
    service : "PAM",
    dev : terraform.workspace != "prod"
  }
}


resource "aws_iam_policy_attachment" "PAM_lambda_kms_policy_attach" {
  name       = "send-email-lambda_kms_policy_attachment-${terraform.workspace}"
  roles      = [aws_iam_role.VH_email_lambda_role.name]
  policy_arn = aws_iam_policy.kms_policy.arn
}

variable "user_pool_arn" {
  default = ""
}
resource "aws_lambda_permission" "allow_cognito_create_challenge" {
  statement_id  = "AllowExecutionFromCognito"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.send_email.function_name
  principal     = "cognito-idp.amazonaws.com"
  source_arn    = var.user_pool_arn
}