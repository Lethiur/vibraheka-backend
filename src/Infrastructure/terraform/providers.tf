provider "aws" {
  region = "eu-west-1"
}

terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
    }
    time = {
      source = "hashicorp/time"
    }
  }

  backend "s3" {
    bucket       = "vibraheka-tf"
    key          = "registration-svc"
    region       = "eu-west-1"
    encrypt      = true
    # use_lockfile = true
  }
}
