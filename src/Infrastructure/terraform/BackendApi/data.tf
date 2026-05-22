# Availability zones used to spread private subnets.
data "aws_availability_zones" "available" {
  state = "available"
}

# Latest Amazon Linux 2023 ARM64 AMI for minimal-cost Graviton instances.
data "aws_ami" "amazon_linux_2023_arm64" {
  most_recent = true
  owners      = ["amazon"]

  filter {
    name   = "name"
    values = ["al2023-ami-2023*-arm64"]
  }

  filter {
    name   = "architecture"
    values = ["arm64"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }
  
  tags = merge(local.tags, {
    Component = "AMI"
  })
}

locals {
  # Workspace normalized to a slug compatible with AWS name constraints.

  repository_name_safe = trim(replace(lower("${var.context.resource_prefix}-repository"), "_", "-"), "-")

  # Suffixes capped to satisfy max length constraints in AWS resources.
  workspace_suffix_34 = substr(var.context.resource_prefix, 0, 34) # 64-char instance profile name limit.
  workspace_suffix_37 = substr(var.context.resource_prefix, 0, 37) # 64-char IAM role name limit.

  iam_role_name    = "${local.workspace_suffix_37}-backend-ec2-role"
  iam_profile_name = "${local.workspace_suffix_34}-backend-ec2-profile"
}
