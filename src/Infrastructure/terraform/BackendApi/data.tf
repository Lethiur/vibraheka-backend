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
