resource "aws_dynamodb_table" "VH_recordings" {
  name         = "VibraHeka-recordings-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "Id"

  attribute {
    name = "Id"
    type = "S"
  }
  
  attribute {
    name = "Tier"
    type = "S"
  }

  global_secondary_index {
    hash_key        = "Tier"
    name            = "tier-index"
    projection_type = "ALL"
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

output "dynamodb_recordings_table_tier_idx" {
  value = one([
    for gsi in aws_dynamodb_table.VH_recordings.global_secondary_index : gsi.name
    if gsi.hash_key == "Tier"
  ])
}
