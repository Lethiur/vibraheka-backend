# ECR repository for backend container images.
resource "aws_ecr_repository" "backend" {
  name                 = local.repository_name_safe
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  force_delete = true

  encryption_configuration {
    encryption_type = "AES256"
  }

  tags = merge(local.tags, {
    Component = "ECR"
    Name = local.repository_name_safe
  })
  
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
