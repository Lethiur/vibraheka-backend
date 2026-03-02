
resource "aws_s3_bucket" "VH_email_templates" {
  bucket = "vibraheka-email-templates-${terraform.workspace}"
  force_destroy = true
}

resource "aws_s3_bucket_public_access_block" "VH_email_templates_access" {
  bucket = aws_s3_bucket.VH_email_templates.id

  block_public_acls       = true
  ignore_public_acls      = true

  # Necesario para permitir una bucket policy pública
  block_public_policy     = false
  restrict_public_buckets = false
}

resource "aws_s3_bucket_ownership_controls" "VH_email_templates_ownership" {
  bucket = aws_s3_bucket.VH_email_templates.id

  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

resource "aws_s3_bucket_policy" "VH_email_templates_public_read_objects" {
  bucket = aws_s3_bucket.VH_email_templates.id

  # CRÍTICO: Esperar a que el candado de Public Access se abra
  depends_on = [aws_s3_bucket_public_access_block.VH_email_templates_access]
  
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "PublicReadOnlySpecificPrefix"
        Effect    = "Allow"
        Principal = "*"
        Action    = ["s3:GetObject"]

        # Lectura pública de TODOS los objetos del bucket
        Resource  = "${aws_s3_bucket.VH_email_templates.arn}/*"
      }
    ]
  })
}

output "s3_email_templates_bucket_name" {
  value = aws_s3_bucket.VH_email_templates.bucket
}

output "s3_email_templates_bucket_arn" {
  value = aws_s3_bucket.VH_email_templates.arn
}