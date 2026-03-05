# ECR repository for backend container images.
resource "aws_ecr_repository" "backend" {
  name                 = "${var.ecr_repository_name}-${terraform.workspace}"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  encryption_configuration {
    encryption_type = "AES256"
  }

  tags = {
    Name        = "${var.ecr_repository_name}-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
    service     = "BackendApi"
  }
}

# Lifecycle policy to avoid unbounded image growth in ECR.
resource "aws_ecr_lifecycle_policy" "backend" {
  repository = aws_ecr_repository.backend.name
  policy = jsonencode({
    rules = [
      {
        rulePriority = 1
        description  = "Keep last 30 images"
        selection = {
          tagStatus   = "any"
          countType   = "imageCountMoreThan"
          countNumber = 30
        }
        action = {
          type = "expire"
        }
      }
    ]
  })
}
