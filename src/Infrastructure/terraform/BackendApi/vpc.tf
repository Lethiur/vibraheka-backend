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

# Single public subnet for the backend host.
resource "aws_subnet" "private_a" {
  vpc_id                  = aws_vpc.backend.id
  cidr_block              = var.private_subnet_a_cidr
  map_public_ip_on_launch = true
  availability_zone       = data.aws_availability_zones.available.names[0]

  tags = {
    Name        = "vibraheka-backend-private-a-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Internet Gateway (public internet egress).
resource "aws_internet_gateway" "backend" {
  vpc_id = aws_vpc.backend.id

  tags = {
    Name        = "vibraheka-backend-igw-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Public route table for subnet A.
resource "aws_route_table" "public" {
  vpc_id = aws_vpc.backend.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.backend.id
  }

  tags = {
    Name        = "vibraheka-backend-public-rt-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Associates subnet A to public route table.
resource "aws_route_table_association" "public_a" {
  subnet_id      = aws_subnet.private_a.id
  route_table_id = aws_route_table.public.id
}
