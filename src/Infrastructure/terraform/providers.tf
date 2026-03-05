provider "aws" {
  region = "eu-west-1"
}

terraform {
  backend "s3" {
    bucket       = "vibraheka-tf"
    key          = "registration-svc"
    region       = "eu-west-1"
    encrypt      = true
    # use_lockfile = true
  }
}
