resource "aws_iam_role" "VH_cognito_ses_role" {
  name = "${local.table_prefix}cognito-ses-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Principal = {
        Service = "cognito-idp.amazonaws.com"
      }
      Action = "sts:AssumeRole"
    }]
  })
  tags = merge(local.tags,{
    Component = "IAM"
  })
}

resource "aws_iam_role_policy" "VH_cognito_ses_policy" {
  name = "${local.table_prefix}cognito-role-email-policy"
  role = aws_iam_role.VH_cognito_ses_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Action = [
        "ses:SendEmail",
        "ses:SendRawEmail"
      ]
      Resource = "*"
    }]
  })
}

