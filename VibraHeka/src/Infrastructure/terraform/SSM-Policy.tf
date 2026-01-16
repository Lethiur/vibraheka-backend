
resource "aws_iam_policy" "VH_ssm_policy" {
  name        = "Policy_Read_App_Settings"
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
        Effect   = "Allow"
        # Esto es lo que te da la libertad: cualquier cosa que empiece por /mi-app/
        Resource = "arn:aws:ssm:*:*:parameter/VibraHeka/*"
      }
    ]
  })
}

resource "aws_ssm_parameter" "VH_ssm_seed_param" {
  name  = "/VibraHeka/AppStatus"
  type  = "String"
  value = "Ready"
}