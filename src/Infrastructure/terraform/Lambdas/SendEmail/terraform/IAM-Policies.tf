resource "aws_iam_role_policy_attachment" "VH_read_ssm_parameter" {
  role = aws_iam_role.VH_email_lambda_role.name
  policy_arn = var.ssm_read_parameter_policy_arn
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
        Resource = [var.ses-domain-arn, var.ses_config_set_arn]
      },
      {
        Effect   = "Allow"
        Action   = ["sesv2:CreateContact"]
        Resource = [var.ses_contact_list_arn]
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


