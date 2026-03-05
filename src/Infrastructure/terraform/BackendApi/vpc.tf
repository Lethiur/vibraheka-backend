# Dedicated VPC for private backend infrastructure.
resource "aws_vpc" "backend" {
  cidr_block           = var.vpc_cidr
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = {
    Name        = "vibraheka-backend-vpc-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
    system      = "VibraHeka"
  }
}

# First private subnet for backend and internal integrations.
resource "aws_subnet" "private_a" {
  vpc_id                  = aws_vpc.backend.id
  cidr_block              = var.private_subnet_a_cidr
  map_public_ip_on_launch = false
  availability_zone       = data.aws_availability_zones.available.names[0]

  tags = {
    Name        = "vibraheka-backend-private-a-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Second private subnet for high availability requirements.
resource "aws_subnet" "private_b" {
  vpc_id                  = aws_vpc.backend.id
  cidr_block              = var.private_subnet_b_cidr
  map_public_ip_on_launch = false
  availability_zone       = data.aws_availability_zones.available.names[1]

  tags = {
    Name        = "vibraheka-backend-private-b-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Private route table shared by backend subnets.
resource "aws_route_table" "private" {
  vpc_id = aws_vpc.backend.id

  tags = {
    Name        = "vibraheka-backend-private-rt-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Associates private subnet A to private route table.
resource "aws_route_table_association" "private_a" {
  subnet_id      = aws_subnet.private_a.id
  route_table_id = aws_route_table.private.id
}

# Associates private subnet B to private route table.
resource "aws_route_table_association" "private_b" {
  subnet_id      = aws_subnet.private_b.id
  route_table_id = aws_route_table.private.id
}

# Security group used by interface VPC endpoints.
resource "aws_security_group" "vpc_endpoints" {
  name        = "vibraheka-backend-vpce-sg-${terraform.workspace}"
  description = "Allow HTTPS traffic from VPC to interface endpoints."
  vpc_id      = aws_vpc.backend.id

  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = [var.vpc_cidr]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "vibraheka-backend-vpce-sg-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Interface endpoint for ECR API calls (GetAuthorizationToken, etc).
resource "aws_vpc_endpoint" "ecr_api" {
  vpc_id              = aws_vpc.backend.id
  service_name        = "com.amazonaws.${data.aws_region.current.region}.ecr.api"
  vpc_endpoint_type   = "Interface"
  subnet_ids          = [aws_subnet.private_a.id, aws_subnet.private_b.id]
  security_group_ids  = [aws_security_group.vpc_endpoints.id]
  private_dns_enabled = true

  tags = {
    Name        = "vibraheka-ecr-api-vpce-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Interface endpoint for ECR Docker registry data plane.
resource "aws_vpc_endpoint" "ecr_dkr" {
  vpc_id              = aws_vpc.backend.id
  service_name        = "com.amazonaws.${data.aws_region.current.region}.ecr.dkr"
  vpc_endpoint_type   = "Interface"
  subnet_ids          = [aws_subnet.private_a.id, aws_subnet.private_b.id]
  security_group_ids  = [aws_security_group.vpc_endpoints.id]
  private_dns_enabled = true

  tags = {
    Name        = "vibraheka-ecr-dkr-vpce-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Gateway endpoint for S3 access used by ECR layer downloads.
resource "aws_vpc_endpoint" "s3" {
  vpc_id            = aws_vpc.backend.id
  service_name      = "com.amazonaws.${data.aws_region.current.region}.s3"
  vpc_endpoint_type = "Gateway"
  route_table_ids   = [aws_route_table.private.id]

  tags = {
    Name        = "vibraheka-s3-vpce-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Interface endpoint for AWS Systems Manager.
resource "aws_vpc_endpoint" "ssm" {
  vpc_id              = aws_vpc.backend.id
  service_name        = "com.amazonaws.${data.aws_region.current.region}.ssm"
  vpc_endpoint_type   = "Interface"
  subnet_ids          = [aws_subnet.private_a.id, aws_subnet.private_b.id]
  security_group_ids  = [aws_security_group.vpc_endpoints.id]
  private_dns_enabled = true

  tags = {
    Name        = "vibraheka-ssm-vpce-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Interface endpoint for SSM Session Manager message channels.
resource "aws_vpc_endpoint" "ssmmessages" {
  vpc_id              = aws_vpc.backend.id
  service_name        = "com.amazonaws.${data.aws_region.current.region}.ssmmessages"
  vpc_endpoint_type   = "Interface"
  subnet_ids          = [aws_subnet.private_a.id, aws_subnet.private_b.id]
  security_group_ids  = [aws_security_group.vpc_endpoints.id]
  private_dns_enabled = true

  tags = {
    Name        = "vibraheka-ssmmessages-vpce-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Interface endpoint for EC2 messages channel required by SSM agent.
resource "aws_vpc_endpoint" "ec2messages" {
  vpc_id              = aws_vpc.backend.id
  service_name        = "com.amazonaws.${data.aws_region.current.region}.ec2messages"
  vpc_endpoint_type   = "Interface"
  subnet_ids          = [aws_subnet.private_a.id, aws_subnet.private_b.id]
  security_group_ids  = [aws_security_group.vpc_endpoints.id]
  private_dns_enabled = true

  tags = {
    Name        = "vibraheka-ec2messages-vpce-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}
