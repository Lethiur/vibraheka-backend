
resource "aws_s3_bucket" "VH_email_templates" {
  bucket = "email-template-${terraform.workspace}" # Cambia esto por un nombre único
}

resource "aws_s3_bucket_public_access_block" "VH_email_templates_access" {
  bucket = aws_s3_bucket.VH_email_templates.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}