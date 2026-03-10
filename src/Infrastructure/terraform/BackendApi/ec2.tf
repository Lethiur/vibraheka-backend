# Security group for the private backend instance.
resource "aws_security_group" "backend_instance" {
  name        = "vibraheka-backend-ec2-sg-${terraform.workspace}"
  description = "Allow backend traffic from API Gateway/NLB private path and SSH for internal management."
  vpc_id      = aws_vpc.backend.id

  # Application traffic from the public internet (cheapest setup).
  ingress {
    from_port   = var.backend_port
    to_port     = var.backend_port
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # SSH management access restricted to private VPC traffic (used with SSM/SSH tunnel from CI).
  dynamic "ingress" {
    for_each = var.enable_ssh_ingress ? [1] : []
    content {
      from_port   = 22
      to_port     = 22
      protocol    = "tcp"
      cidr_blocks = var.ssh_allowed_cidrs
    }
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
  name = local.iam_role_name

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
  name = local.iam_profile_name
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

# Single public EC2 instance with Elastic IP (minimal VPC setup).
resource "aws_instance" "backend" {
  ami                         = data.aws_ami.amazon_linux_2023_arm64.id
  instance_type               = var.backend_instance_type
  subnet_id                   = aws_subnet.private_a.id
  vpc_security_group_ids      = [aws_security_group.backend_instance.id]
  associate_public_ip_address = true
  iam_instance_profile        = aws_iam_instance_profile.backend.name
  key_name                    = aws_key_pair.backend.key_name

  user_data = <<-EOF
              #!/bin/bash
              set -euxo pipefail

              mkdir -p /opt/backend
              echo "VibraHeka backend host ready for container deployment in ${terraform.workspace}" > /opt/backend/host-ready.txt

              # Fail2ban: basic SSH brute-force protection.
              if ! dnf -y install fail2ban; then
                dnf -y install epel-release || true
                dnf -y install fail2ban || true
              fi
              mkdir -p /etc/fail2ban/jail.d
              cat > /etc/fail2ban/jail.d/sshd.local <<'JAIL'
              [sshd]
              enabled = true
              backend = systemd
              bantime = 1h
              findtime = 10m
              maxretry = 5
              JAIL
              systemctl enable --now fail2ban || true
              EOF

  tags = {
    Name        = "vibraheka-backend-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
    service     = "BackendApi"
  }
}

resource "aws_eip" "backend" {
  domain = "vpc"

  tags = {
    Name        = "vibraheka-backend-eip-${terraform.workspace}"
    environment = terraform.workspace
    created     = "terraform"
    service     = "BackendApi"
  }
}

resource "aws_eip_association" "backend" {
  instance_id   = aws_instance.backend.id
  allocation_id = aws_eip.backend.id
}
