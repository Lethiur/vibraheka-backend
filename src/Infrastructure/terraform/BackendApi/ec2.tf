locals {
  # If caller does not provide SSH CIDRs, default to internal VPC range.
  effective_ssh_cidrs = length(var.ssh_allowed_cidrs) > 0 ? var.ssh_allowed_cidrs : [var.vpc_cidr]
  # Explicit API Gateway routes rendered as a text file in EC2 for operational traceability.
  api_gateway_routes_text = join("\n", sort(tolist(var.api_gateway_explicit_routes)))
}

# Security group for the private backend instance.
resource "aws_security_group" "backend_instance" {
  name        = "vibraheka-backend-ec2-sg-${terraform.workspace}"
  description = "Allow backend traffic from API Gateway/NLB path and SSH management traffic."
  vpc_id      = aws_vpc.backend.id

  # Application traffic from private subnets where NLB nodes live.
  ingress {
    from_port   = var.backend_port
    to_port     = var.backend_port
    protocol    = "tcp"
    cidr_blocks = [var.private_subnet_a_cidr, var.private_subnet_b_cidr]
  }

  # SSH management traffic (for SSM tunnel / bastion / private connectivity use cases).
  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = local.effective_ssh_cidrs
  }

  # Egress to AWS services and package/image endpoints.
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "vibraheka-backend-ec2-sg-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# IAM role assumed by EC2 for SSM and ECR image pull operations.
resource "aws_iam_role" "backend_ec2_role" {
  name = "vibraheka-backend-ec2-role-${terraform.workspace}"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ec2.amazonaws.com"
        }
      }
    ]
  })
}

# Managed policy for Session Manager connectivity.
resource "aws_iam_role_policy_attachment" "ssm_core" {
  role       = aws_iam_role.backend_ec2_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonSSMManagedInstanceCore"
}

# Managed policy for pulling images from ECR.
resource "aws_iam_role_policy_attachment" "ecr_read_only" {
  role       = aws_iam_role.backend_ec2_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly"
}

# Instance profile binding EC2 role to the instance.
resource "aws_iam_instance_profile" "backend" {
  name = "vibraheka-backend-ec2-profile-${terraform.workspace}"
  role = aws_iam_role.backend_ec2_role.name
}

# Optional generated private key for CI/CD SSH usage.
resource "tls_private_key" "backend_ssh" {
  count     = var.create_ssh_key_pair ? 1 : 0
  algorithm = "RSA"
  rsa_bits  = 4096
}

# Key pair registered in EC2 for SSH.
resource "aws_key_pair" "backend" {
  key_name   = "vibraheka-backend-ssh-${terraform.workspace}"
  public_key = var.create_ssh_key_pair ? tls_private_key.backend_ssh[0].public_key_openssh : var.existing_ssh_public_key
}

# Generated private key stored securely for retrieval from CI (GitHub Actions).
resource "aws_ssm_parameter" "backend_ssh_private_key" {
  count = var.create_ssh_key_pair ? 1 : 0

  name        = "${var.ssh_private_key_ssm_parameter_name_prefix}/${terraform.workspace}"
  description = "Private SSH key for backend EC2 instance in ${terraform.workspace}."
  type        = "SecureString"
  value       = tls_private_key.backend_ssh[0].private_key_pem
  overwrite   = true

  tags = {
    Name        = "vibraheka-backend-ssh-key-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
  }
}

# Private EC2 spot instance hosting backend app.
resource "aws_instance" "backend_spot" {
  ami                         = data.aws_ami.amazon_linux_2023_arm64.id
  instance_type               = var.backend_instance_type
  subnet_id                   = aws_subnet.private_a.id
  vpc_security_group_ids      = [aws_security_group.backend_instance.id]
  associate_public_ip_address = false
  iam_instance_profile        = aws_iam_instance_profile.backend.name
  key_name                    = aws_key_pair.backend.key_name

  instance_market_options {
    market_type = "spot"
    spot_options {
      spot_instance_type = "one-time"
    }
  }

  user_data = <<-EOF
              #!/bin/bash
              cat <<'SERVICE' > /etc/systemd/system/backend-http.service
              [Unit]
              Description=Simple backend endpoint for API Gateway proxy
              After=network.target

              [Service]
              Type=simple
              User=root
              WorkingDirectory=/opt/backend
              ExecStart=/usr/bin/python3 -m http.server ${var.backend_port} --bind 0.0.0.0
              Restart=always
              RestartSec=3

              [Install]
              WantedBy=multi-user.target
              SERVICE

              mkdir -p /opt/backend
              echo "VibraHeka backend host up for ${terraform.workspace}" > /opt/backend/index.html
              cat <<'ROUTES' > /opt/backend/api-gateway-routes.txt
              ${local.api_gateway_routes_text}
              ROUTES
              systemctl daemon-reload
              systemctl enable backend-http.service
              systemctl start backend-http.service
              EOF

  tags = {
    Name        = "vibraheka-backend-spot-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
    service     = "BackendApi"
  }
}
