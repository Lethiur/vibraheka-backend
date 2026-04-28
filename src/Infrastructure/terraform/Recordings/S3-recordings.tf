resource "aws_s3_bucket" "VH_recordings" {
  bucket        = "vibraheka-recordings-${terraform.workspace}"
  force_destroy = true

  tags = {
    created     = "terraform"
    environment = terraform.workspace
    system      = "VibraHeka"
    service     = "Recordings"
    dev         = tostring(terraform.workspace != "prod")
  }
}

# Block ALL public access — bucket is fully private
resource "aws_s3_bucket_public_access_block" "VH_recordings_access" {
  bucket = aws_s3_bucket.VH_recordings.id

  block_public_acls       = true
  ignore_public_acls      = true
  block_public_policy     = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_ownership_controls" "VH_recordings_ownership" {
  bucket = aws_s3_bucket.VH_recordings.id

  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

resource "aws_s3_bucket_cors_configuration" "VH_recordings_cors" {
  bucket = aws_s3_bucket.VH_recordings.id

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["PUT", "POST", "GET"]
    allowed_origins = ["*"]
    expose_headers  = []
  }
}

output "s3_recordings_bucket_name" {
  value = aws_s3_bucket.VH_recordings.bucket
}

output "s3_recordings_bucket_arn" {
  value = aws_s3_bucket.VH_recordings.arn
}
