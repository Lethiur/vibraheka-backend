resource "aws_s3_bucket" "VH_user_material" {
  bucket = "vibraheka-user-material-${terraform.workspace}"
}

resource "aws_s3_bucket_public_access_block" "VH_user_material_access" {
  bucket = aws_s3_bucket.VH_user_material.id

  block_public_acls       = true
  ignore_public_acls      = true

  # Necesario para permitir una bucket policy pública
  block_public_policy     = false
  restrict_public_buckets = false
}

resource "aws_s3_bucket_ownership_controls" "VH_user_material_ownership" {
  bucket = aws_s3_bucket.VH_user_material.id

  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

resource "aws_s3_bucket_policy" "VH_user_material_public_read_objects" {
  bucket = aws_s3_bucket.VH_user_material.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "PublicReadOnlySpecificPrefix"
        Effect    = "Allow"
        Principal = "*"
        Action    = ["s3:GetObject"]

        # Lectura pública de TODOS los objetos del bucket
        Resource  = "${aws_s3_bucket.VH_user_material.arn}/*"
      }
    ]
  })
}
