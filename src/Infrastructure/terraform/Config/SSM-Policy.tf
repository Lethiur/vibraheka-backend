
resource "aws_iam_policy" "SSM_Policy" {
  name        = "${var.context.resource_prefix}-Policy-Read-App-Settings"
  description = "Permite leer CUALQUIER cosa dentro de /mi-app/ sin tocar Terraform"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "ssm:GetParametersByPath",
          "ssm:GetParameter",
          "ssm:GetParameters"
        ]
        Effect = "Allow"
        # Esto es lo que te da la libertad: cualquier cosa que empiece por /mi-app/
        Resource = "arn:aws:ssm:*:*:parameter/${var.ssm_namespace}/*"
      }
    ]
  })

  tags = merge(local.tags, {
    Component = "IAM"
  })
}
