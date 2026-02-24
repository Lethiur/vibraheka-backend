
resource "aws_dynamodb_table" "VH_email_templates" {
  name         = "VibraHeka-EmailTempaltes-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "ID"

  attribute {
    name = "ID"
    type = "S"
  }
  tags = {
    created : "terraform",
    environment : terraform.workspace,
    system : "VibraHeka",
    service : "Emails",
    dev : terraform.workspace != "prod"
  }
}

output "email_templates_table_name" {
  value = aws_dynamodb_table.VH_email_templates.name
}
