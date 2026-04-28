
data "aws_caller_identity" "current" {}

data "aws_region" "current" {}

locals {
  manage_shared_ses = terraform.workspace == "main"
  ses_domain_name   = "vibraheka.com"
  ses_from_email    = "heka@${local.ses_domain_name}"
  ses_domain_arn    = "arn:aws:ses:${data.aws_region.current.name}:${data.aws_caller_identity.current.account_id}:identity/heka@${local.ses_domain_name}"
}

resource "aws_ses_domain_identity" "VibraHeka_ses_domain" {
  count  = local.manage_shared_ses ? 1 : 0
  domain = local.ses_domain_name
}

resource "time_sleep" "wait_for_ses_domain_identity" {
  count      = local.manage_shared_ses ? 1 : 0
  depends_on = [aws_ses_domain_identity.VibraHeka_ses_domain]

  create_duration = "30s"
}

resource "aws_ses_domain_dkim" "VibraHeka_ses_dkim" {
  count      = local.manage_shared_ses ? 1 : 0
  depends_on = [time_sleep.wait_for_ses_domain_identity]

  domain = aws_ses_domain_identity.VibraHeka_ses_domain[0].domain
}

resource "aws_ses_configuration_set" "VibraHeka_ses_config" {
  name = "VibraHeka-ses-config-${terraform.workspace}"
}

resource "aws_sesv2_contact_list" "VibraHeka_contacts" {
  contact_list_name = "VibraHeka-contacts-${terraform.workspace}"
}

resource "aws_ses_domain_mail_from" "VibraHeka_ses_tracking" {
  count      = local.manage_shared_ses ? 1 : 0
  depends_on = [time_sleep.wait_for_ses_domain_identity]

  domain           = aws_ses_domain_identity.VibraHeka_ses_domain[0].domain
  mail_from_domain = "mail.${local.ses_domain_name}"
}

output "ses_config_arn" {
  value = aws_ses_configuration_set.VibraHeka_ses_config.arn
}

output "ses_config_name" {
  value = aws_ses_configuration_set.VibraHeka_ses_config.name
}

output "ses_from_email" {
  value = local.ses_from_email
}

output "ses_email_domain_arn" {
  value = local.ses_domain_arn
}

output "ses_domain_identity_name" {
  value = try(aws_ses_domain_identity.VibraHeka_ses_domain[0].domain, null)
}

output "ses_domain_identity_verification_record_name" {
  value = "_amazonses.${local.ses_domain_name}"
}

output "ses_contact_list_name" {
  value = aws_sesv2_contact_list.VibraHeka_contacts.contact_list_name
}

output "ses_contact_list_arn" {
  value = aws_sesv2_contact_list.VibraHeka_contacts.arn
}
