resource "aws_dynamodb_table" "VH_recordings" {
  name         = "VibraHeka-recordings-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"

  attribute {
    name = "Id"
    type = "S"
  }

  tags = {
    created     = "terraform"
    environment = terraform.workspace
    system      = "VibraHeka"
    service     = "Recordings"
    dev         = tostring(terraform.workspace != "prod")
  }
}

output "dynamodb_recordings_table_name" {
  value = aws_dynamodb_table.VH_recordings.name
}

output "dynamodb_recordings_table_arn" {
  value = aws_dynamodb_table.VH_recordings.arn
}
