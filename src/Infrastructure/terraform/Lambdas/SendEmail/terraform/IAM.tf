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
        Resource = var.ses-arn
      }
    ]
  })
}


resource "aws_iam_role_policy" "VH_s3_bucket_access" {
  name = "s3-read-templates-policy"
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