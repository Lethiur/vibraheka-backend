provider "aws" {
  profile = "Twingers"
}

terraform {
  backend "s3" {
    bucket       = "vibraheka-tf"
    key          = "registration-svc"
    region       = "eu-west-1"
    profile      = "Twingers"
    encrypt      = true
    use_lockfile = true
  }
}
