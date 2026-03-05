# Availability zones used to spread private subnets.
data "aws_availability_zones" "available" {
  state = "available"
}

# Current AWS region (used to build VPC endpoint service names).
data "aws_region" "current" {}

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
}

locals {
  # Workspace normalized to a slug compatible with AWS name constraints.
  workspace_slug = trim(replace(lower(terraform.workspace), "_", "-"), "-")
  workspace_safe = local.workspace_slug != "" ? local.workspace_slug : "default"
  workspace_hash = substr(md5(local.workspace_safe), 0, 6)

  # Suffixes capped to satisfy max length constraints in AWS resources.
  workspace_suffix_8  = substr(local.workspace_safe, 0, 8) # Short readable suffix for LB/TG names.
  workspace_suffix_34 = substr(local.workspace_safe, 0, 34) # 64-char instance profile name limit.
  workspace_suffix_37 = substr(local.workspace_safe, 0, 37) # 64-char IAM role name limit.

  lb_name           = "vibraheka-be-nlb-${local.workspace_suffix_8}-${local.workspace_hash}"
  target_group_name = "vibraheka-be-tg-${local.workspace_suffix_8}-${local.workspace_hash}"
  iam_role_name    = "vibraheka-backend-ec2-role-${local.workspace_suffix_37}"
  iam_profile_name = "vibraheka-backend-ec2-profile-${local.workspace_suffix_34}"
}
