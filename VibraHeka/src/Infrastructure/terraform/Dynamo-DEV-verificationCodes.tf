resource "aws_dynamodb_table" "VibraHeka_PAM_verification_codes" {
  name         = "VibraHeka-CognitoVerificationCodes-${terraform.workspace}"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "username"

  attribute {
    name = "username"
    type = "S"
  }
  tags = {
    "created" : "terraform", "environment" : terraform.workspace, "service" : "email-sending",
    dev : terraform.workspace != "prod"
  }
}

output "verification_codes_table" {
  value = aws_dynamodb_table.VibraHeka_PAM_verification_codes.name 
  description = "The name of the terraform table that stores the codes. Only available in test environment"
}